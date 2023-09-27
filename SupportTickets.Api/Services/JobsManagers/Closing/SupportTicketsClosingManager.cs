using AutoMapper;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using SupportTickets.Api.Models.SupportTickets;
using SupportTickets.Api.Models.SupportTicketStatusRecords;
using SupportTickets.Api.Repositories.SupportTickets;
using SupportTickets.Api.Repositories.SupportTicketStatusRecords;

namespace SupportTickets.Api.Services.JobsManagers.Closing;

public class SupportTicketsClosingManager : ISupportTicketsClosingManager
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly ISupportTicketsRepository _supportTicketsRepository;
    private readonly ISupportTicketStatusRecordsRepository _statusRecordsRepository;
    private readonly IMapper _mapper;

    public SupportTicketsClosingManager(
        IBackgroundJobClient jobClient, 
        ISupportTicketsRepository supportTicketsRepository, 
        ISupportTicketStatusRecordsRepository statusRecordsRepository, 
        IMapper mapper)
    {
        _jobClient = jobClient;
        _supportTicketsRepository = supportTicketsRepository;
        _statusRecordsRepository = statusRecordsRepository;
        _mapper = mapper;
    }

    public void EnsureAssignCloseFor(Guid supportTicketId, TimeSpan afterTime)
    {
        if (GetEscalationJobId(supportTicketId) != null)
            return;

        _jobClient.Schedule(() => EnsureCloseAsync(supportTicketId), afterTime);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    // Hangfire requires public methods for jobs
    public async Task EnsureCloseAsync(Guid supportTicketId)
    {
        SupportTicket supportTicket = (await _supportTicketsRepository.GetByIdAsync(supportTicketId))!;

        if (supportTicket.Status == SupportTicketStatus.Close || supportTicket.Agent == null)
            return;

        supportTicket.Status = SupportTicketStatus.Close;
        await _supportTicketsRepository.UpdateAsync(supportTicket);
        
        var statusRecord = _mapper.Map<SupportTicketStatusRecord>(supportTicket);
        statusRecord.DateTime = DateTime.Now;
        await _statusRecordsRepository.InsertAsync(statusRecord);
    }

    public void EnsureCancelCloseFor(Guid supportTicketId)
    {
        string? jobId = GetEscalationJobId(supportTicketId);
        if (jobId == null)
            return;
        _jobClient.Delete(jobId);
    }

    private static string? GetEscalationJobId(Guid supportTicketId)
    {
        IMonitoringApi monitor = JobStorage.Current.GetMonitoringApi();
        IEnumerable<KeyValuePair<string, ScheduledJobDto>> jobsScheduled =
            monitor
                .ScheduledJobs(from: 0, count: int.MaxValue)
                .Where(_ => _.Value.Job.Method.Name == nameof(EnsureCloseAsync));

        string? jobId = null;
        foreach (KeyValuePair<string, ScheduledJobDto> keyValuePair in jobsScheduled)
        {
            var id = (Guid)keyValuePair.Value.Job.Args[0];
            if (id != supportTicketId) continue;
            jobId = keyValuePair.Key;
        }

        return jobId;
    }
}
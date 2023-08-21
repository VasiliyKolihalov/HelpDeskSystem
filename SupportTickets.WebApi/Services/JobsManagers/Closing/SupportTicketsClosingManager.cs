using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Repositories.SupportTickets;

namespace SupportTickets.WebApi.Services.JobsManagers.Closing;

public class SupportTicketsClosingManager : ISupportTicketsClosingManager
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly ISupportTicketsRepository _supportTicketsRepository;
    private readonly Dictionary<Guid, string> _jobIds = new();

    public SupportTicketsClosingManager(IBackgroundJobClient jobClient,
        ISupportTicketsRepository supportTicketsRepository)
    {
        _jobClient = jobClient;
        _supportTicketsRepository = supportTicketsRepository;
    }

    public void EnsureAssignCloseFor(Guid supportTicketId, TimeSpan afterTime)
    {
        if (GetEscalationJobId(supportTicketId) != null)
            return;

        _jobIds[supportTicketId] = _jobClient.Schedule(() => EnsureCloseAsync(supportTicketId), afterTime);
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
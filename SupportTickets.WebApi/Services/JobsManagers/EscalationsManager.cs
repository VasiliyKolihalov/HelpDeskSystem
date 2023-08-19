using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Repositories.SupportTickets;

namespace SupportTickets.WebApi.Services.JobsManagers;

public class EscalationsManager : IEscalationsManager
{
    private readonly ISupportTicketsRepository _supportTicketsRepository;
    private readonly IBackgroundJobClient _jobClient;

    public EscalationsManager(ISupportTicketsRepository supportTicketsRepository, IBackgroundJobClient jobClient)
    {
        _supportTicketsRepository = supportTicketsRepository;
        _jobClient = jobClient;
    }

    public void AssignEscalationFor(Guid supportTicketId, TimeSpan afterTime)
    {
        _jobClient.Schedule(() => EscalateAsync(supportTicketId), afterTime);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    // Hangfire requires public methods for jobs
    public async Task EscalateAsync(Guid supportTicketId)
    {
        SupportTicket supportTicket = (await _supportTicketsRepository.GetByIdAsync(supportTicketId))!;
        supportTicket.Agent = default;
        supportTicket.Priority = supportTicket.Priority switch
        {
            SupportTicketPriority.Low => SupportTicketPriority.Medium,
            SupportTicketPriority.Medium => SupportTicketPriority.High,
            SupportTicketPriority.High => SupportTicketPriority.High,
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(SupportTicket.Priority),
                message: "Unknown SupportTicketPriority")
        };
        await _supportTicketsRepository.UpdateAsync(supportTicket);
    }

    public void CancelEscalationFor(Guid supportTicketId)
    {
        IMonitoringApi monitor = JobStorage.Current.GetMonitoringApi();
        IEnumerable<KeyValuePair<string, ScheduledJobDto>> jobsScheduled =
            monitor
                .ScheduledJobs(from: 0, count: int.MaxValue)
                .Where(_ => _.Value.Job.Method.Name == nameof(EscalateAsync));

        foreach (KeyValuePair<string, ScheduledJobDto> keyValuePair in jobsScheduled)
        {
            var id = (Guid)keyValuePair.Value.Job.Args[0];
            if (id == supportTicketId)
            {
                _jobClient.Delete(keyValuePair.Key);
            }
        }
    }
}
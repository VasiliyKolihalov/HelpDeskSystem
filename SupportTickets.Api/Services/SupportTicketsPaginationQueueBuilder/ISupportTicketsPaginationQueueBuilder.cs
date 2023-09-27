using SupportTickets.Api.Models.SupportTickets;

namespace SupportTickets.Api.Services.SupportTicketsPaginationQueueBuilder;

public interface ISupportTicketsPaginationQueueBuilder
{
    public ISupportTicketsPaginationQueueBuilder WhereStatusEquals(SupportTicketStatus status);
    public ISupportTicketsPaginationQueueBuilder WherePriorityEquals(SupportTicketPriority priority);
    public ISupportTicketsPaginationQueueBuilder WhereAgentIdIsNull();
    public ISupportTicketsPaginationQueueBuilder WhereUserIdOrAgentIdEquals(Guid id);
    public ISupportTicketsPaginationQueueBuilder LimitAndOffset(int limit, int offset);
}
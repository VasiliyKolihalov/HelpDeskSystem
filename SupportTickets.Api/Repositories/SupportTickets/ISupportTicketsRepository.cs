using Infrastructure.Repositories;
using SupportTickets.Api.Models.SupportTickets;
using SupportTickets.Api.Services.SupportTicketsPaginationQueueBuilder;

namespace SupportTickets.Api.Repositories.SupportTickets;

public interface ISupportTicketsRepository : IRepository<SupportTicket, Guid>
{
    public Task<IEnumerable<SupportTicket>> GetBasedOnAccountAsync(Guid accountId);
    public Task<IEnumerable<SupportTicket>> GetAllOpenWithoutAgent();

    public Task<IEnumerable<SupportTicket>> GetByPagination(
        Action<ISupportTicketsPaginationQueueBuilder> builderAction);

    public Task<int> GetCountByPagination(
        Action<ISupportTicketsPaginationQueueBuilder> builderAction);
}
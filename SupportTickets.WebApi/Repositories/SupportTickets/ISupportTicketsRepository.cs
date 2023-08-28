using Infrastructure.Repositories;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Services.SupportTicketsPaginationQueueBuilder;

namespace SupportTickets.WebApi.Repositories.SupportTickets;

public interface ISupportTicketsRepository : IRepository<SupportTicket, Guid>
{
    public Task<IEnumerable<SupportTicket>> GetBasedOnAccountAsync(Guid accountId);
    public Task<IEnumerable<SupportTicket>> GetAllOpenWithoutAgent();

    public Task<IEnumerable<SupportTicket>> GetByPagination(
        Action<ISupportTicketsPaginationQueueBuilder> builderAction);

    public Task<int> GetCountByPagination(
        Action<ISupportTicketsPaginationQueueBuilder> builderAction);
}
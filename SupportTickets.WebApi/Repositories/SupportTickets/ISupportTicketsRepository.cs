using Infrastructure.Repositories;
using SupportTickets.WebApi.Models.SupportTickets;

namespace SupportTickets.WebApi.Repositories.SupportTickets;

public interface ISupportTicketsRepository : IRepository<SupportTicket, Guid>
{
}
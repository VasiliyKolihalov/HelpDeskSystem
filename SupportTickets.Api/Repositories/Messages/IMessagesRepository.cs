using Infrastructure.Repositories;
using SupportTickets.Api.Models.Messages;

namespace SupportTickets.Api.Repositories.Messages;

public interface IMessagesRepository : IRepository<Message, Guid>
{
    
}
using Infrastructure.Repositories;
using SupportTickets.WebApi.Models.Messages;

namespace SupportTickets.WebApi.Repositories.Messages;

public interface IMessagesRepository : IRepository<Message, Guid>
{
    
}
﻿using Infrastructure.Repositories;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.SupportTickets;

namespace SupportTickets.WebApi.Repositories.SupportTickets;

public interface ISupportTicketsRepository : IRepository<SupportTicket, Guid>
{
    public Task<IEnumerable<SupportTicket>> GetBasedOnAccountAsync(Guid accountId);

    public Task<Message?> GetMessageByIdAsync(Guid messageId);
    public Task AddMessageAsync(Message message);
    public Task UpdateMessageAsync(Message message);
    public Task DeleteMessageAsync(Guid messageId);
}
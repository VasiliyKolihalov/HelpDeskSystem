using Authentication.Infrastructure.Models;

namespace SupportTickets.WebApi.Services.Clients;

public interface IAccountsClient
{
    public Task<Account<Guid>> SendGetRequestAsync(Guid accountId);
}
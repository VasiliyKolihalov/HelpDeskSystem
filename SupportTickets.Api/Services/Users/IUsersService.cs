using SupportTickets.Api.Models.Users;

namespace SupportTickets.Api.Services.Users;

public interface IUsersService
{
    public Task CreateAsync(User user);
    public Task UpdateAsync(User user);
    public Task DeleteAsync(Guid id);
}
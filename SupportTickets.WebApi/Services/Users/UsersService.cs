using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Repositories.Users;

namespace SupportTickets.WebApi.Services.Users;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;

    public UsersService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task CreateAsync(User user)
    {
        await _usersRepository.InsertAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _usersRepository.UpdateAsync(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _usersRepository.DeleteAsync(id);
    }
}
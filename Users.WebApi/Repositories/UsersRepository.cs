using Microsoft.EntityFrameworkCore;
using Users.WebApi.Models.Users;

namespace Users.WebApi.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly ApplicationContext _applicationContext;

    public UsersRepository(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _applicationContext.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        User? user = await _applicationContext.Users.FirstOrDefaultAsync(_ => _.Id == id);
        return user;
    }

    public async Task<bool> IsExistsAsync(Guid id)
    {
        return await _applicationContext.Users.AnyAsync(_ => _.Id == id);
    }

    public async Task InsertAsync(User item)
    {
        await _applicationContext.Users.AddAsync(item);
        await _applicationContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(User item)
    {
        _applicationContext.Users.Update(item);
        await _applicationContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = new User { Id = id };
        _applicationContext.Users.Attach(user);
        _applicationContext.Users.Remove(user);
        await _applicationContext.SaveChangesAsync();
    }
}
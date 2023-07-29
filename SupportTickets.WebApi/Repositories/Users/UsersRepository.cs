using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.Users;

public class UsersRepository : IUsersRepository
{
    private readonly IDbContext _dbContext;

    public UsersRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string query = "select * from users where id = @id";
        User? user = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            user = await transaction.QueryFirstOrDefaultAsync<User>(query, param: new { id });
        });
        return user;
    }

    public async Task InsertAsync(User user)
    {
        const string query = "insert into users values (@Id, @FirstName, @LastName)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, user));
    }

    public async Task UpdateAsync(User user)
    {
        const string query = "update users set firstname = @FirstName, lastName = @LastName where id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: user);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from users where id = @id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: new { id });
        });
    }
}
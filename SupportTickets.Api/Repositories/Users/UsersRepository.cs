using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using SupportTickets.Api.Models.Users;

namespace SupportTickets.Api.Repositories.Users;

public class UsersRepository : IUsersRepository
{
    private readonly IDbContext _dbContext;

    public UsersRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string query = "select * from Users";
        IEnumerable<User> users = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            users = await transaction.QueryAsync<User>(query);
        });
        return users;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string query = "select * from Users where Id = @id";
        User? user = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            user = await transaction.QueryFirstOrDefaultAsync<User>(query, param: new { id });
        });
        return user;
    }

    public async Task<bool> IsExistsAsync(Guid id)
    {
        const string query = "select exists(select * from Users where Id = @id)";
        await using DbConnection connection = _dbContext.CreateConnection();
        var exists = false;
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            exists = await transaction.QuerySingleAsync<bool>(query, new { id });
        });
        return exists;
    }

    public async Task InsertAsync(User user)
    {
        const string query = "insert into Users values (@Id, @FirstName, @LastName)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, user));
    }

    public async Task UpdateAsync(User user)
    {
        const string query = "update Users set Firstname = @FirstName, LastName = @LastName where Id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: user);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from Users where Id = @id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: new { id });
        });
    }
}
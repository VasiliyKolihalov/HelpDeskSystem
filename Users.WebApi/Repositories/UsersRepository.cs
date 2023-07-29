using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using Users.WebApi.Models.Users;

namespace Users.WebApi.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly IDbContext _dbContext;

    public UsersRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string query = "select * from users";
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
        const string query = "select * from users where id = @id";
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

    public async Task InsertAsync(User item)
    {
        const string query = "insert into users values (@Id, @FirstName, @LastName, @Email)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task UpdateAsync(User item)
    {
        const string query = @"update users set 
                            firstName = @FirstName, 
                            lastName = @LastName, 
                            email = @Email 
                            where id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
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
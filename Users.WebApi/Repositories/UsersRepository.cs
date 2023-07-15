using Dapper.Transaction;
using Infrastructure.Extensions;
using Npgsql;
using Users.WebApi.Models.Users;

namespace Users.WebApi.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly string _connectionString;

    public UsersRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string query = "select * from Users";
        IEnumerable<User> users = null!;
        await using var connection = new NpgsqlConnection(_connectionString);
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
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            user = await transaction.QueryFirstOrDefaultAsync<User>(query, param: new { id });
        });
        return user;
    }

    public async Task InsertAsync(User item)
    {
        const string query = "insert into Users values (@Id, @FirstName, @LastName, @Email)";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task UpdateAsync(User item)
    {
        const string query = @"update Users set 
                            FirstName = @FirstName, 
                            LastName = @LastName, 
                            Email = @Email 
                            where Id = @Id";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from Users where Id = @id";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: new { id });
        });
    }
}
using Dapper.Transaction;
using Infrastructure.Extensions;
using Npgsql;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.Users;

public class UsersRepository : IUsersRepository
{
    private readonly string _connectionString;

    public UsersRepository(string connectionString)
    {
        _connectionString = connectionString;
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

    public async Task InsertAsync(User user)
    {
        const string query = "insert into Users values (@Id, @FirstName, @LastName)";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, user));
    }

    public async Task UpdateAsync(User user)
    {
        const string query = "update Users set Firstname = @FirstName, LastName = @LastName where Id = @Id";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: user);
        });
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
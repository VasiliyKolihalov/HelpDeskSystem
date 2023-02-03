using System.Data.SqlClient;
using Dapper.Transaction;
using HelpDeskSystem.Exceptions;
using HelpDeskSystem.Extensions;
using HelpDeskSystem.Models.User;

namespace HelpDeskSystem.Repositories;

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
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            users = await transaction.QueryAsync<User>(query);
        });
        return users;
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        const string query = "select * from Users where Id = @id";
        User? user = null;
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            user = await transaction.QuerySingleOrDefaultAsync<User>(query, new { id });
        });
        return user ?? throw new NotFoundException("User ticket not found");
    }

    public async Task InsertAsync(User item)
    {
        const string query = "insert into Users values(@Id, @FirstName, @LastName, @Email, @PasswordHash)";
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task UpdateAsync(User item)
    {
        const string query = @"update Users set 
                            FirstName = @FirstName, 
                            LastName = @LastName
                            where Id = @Id";

        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from Users where Id = @id";
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, new { id });
        });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string query = "select * from Users where Email = @email";
        await using var connection = new SqlConnection(_connectionString);
        User? user = null;
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            user = await transaction.QuerySingleOrDefaultAsync<User>(query, new { email });
        });
        return user;
    }
}
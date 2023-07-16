using System.Data.Common;
using Npgsql;

namespace Infrastructure.Services.Persistence;

public class PostgresContext : IDbContext
{
    private readonly string _connectionString;
    private readonly string _masterConnectionString;

    public PostgresContext(string connectionString, string masterConnectionString)
    {
        _connectionString = connectionString;
        _masterConnectionString = masterConnectionString;
    }

    public DbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public DbConnection CreateMasterConnection()
    {
        return new NpgsqlConnection(_masterConnectionString);
    }
}
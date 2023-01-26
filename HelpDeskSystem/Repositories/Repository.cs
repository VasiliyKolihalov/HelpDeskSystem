using System.Data.Common;
using System.Data.SqlClient;

namespace HelpDeskSystem.Repositories;

public abstract class Repository<TItem, TId>
{
    private readonly string _connectionString;

    protected Repository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public abstract Task<IEnumerable<TItem>> GetAllAsync();
    public abstract Task<TItem> GetByIdAsync(TId id);
    public abstract Task InsertAsync(TItem item);
    public abstract Task UpdateAsync(TItem item);
    public abstract Task DeleteAsync(TId id);

    protected async Task ExecuteTransactionAsync(Func<DbTransaction, Task> action)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        await action.Invoke(transaction);
        await transaction.CommitAsync();
    }
}
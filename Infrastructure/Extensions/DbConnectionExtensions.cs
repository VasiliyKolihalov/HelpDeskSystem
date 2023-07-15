using System.Data.Common;
namespace Infrastructure.Extensions;

public static class DbConnectionExtensions
{
    public static async Task ExecuteTransactionAsync(this DbConnection @this, Func<DbTransaction, Task> action)
    {
        await @this.OpenAsync();
        await using DbTransaction transaction = await @this.BeginTransactionAsync();
        await action.Invoke(transaction);
        await transaction.CommitAsync();
    }
}
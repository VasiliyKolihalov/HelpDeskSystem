using System.Data.Common;
using System.Data.SqlClient;

namespace HelpDeskSystem.Extensions;

public static class SqlConnectionExtensions
{
    public static async Task ExecuteTransactionAsync(this SqlConnection @this, Func<DbTransaction, Task> action)
    {
        await @this.OpenAsync();
        await using DbTransaction transaction = await  @this.BeginTransactionAsync();
        await action.Invoke(transaction);
        await transaction.CommitAsync();
    }
}
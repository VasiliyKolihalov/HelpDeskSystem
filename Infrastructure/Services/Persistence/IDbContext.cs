using System.Data.Common;

namespace Infrastructure.Services.Persistence;

public interface IDbContext
{
    public DbConnection CreateConnection();
    public DbConnection CreateMasterConnection();
}
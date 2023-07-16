using System.Data.Common;
using Dapper;
using FluentMigrator.Runner;
using Infrastructure.Services.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Extensions;

public static class HostExtensions
{
    public static async Task<IHost> UseFluentMigrationAsync(
        this IHost @this,
        Func<MigrationOptions, Task>? options = null)
    {
        using IServiceScope scope = @this.Services.CreateScope();
        if (options != null)
        {
            var migrationOptions = new MigrationOptions(
                dbContext: scope.ServiceProvider.GetRequiredService<IDbContext>(),
                logger: scope.ServiceProvider.GetRequiredService<ILogger<MigrationOptions>>());
            await options.Invoke(migrationOptions);
        }

        var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        migrationService.ListMigrations();
        migrationService.MigrateUp();
        return @this;
    }

    public class MigrationOptions
    {
        private readonly IDbContext _dbContext;
        private readonly ILogger<MigrationOptions> _logger;

        public MigrationOptions(IDbContext dbContext, ILogger<MigrationOptions> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task CreateDatabaseAsync(string name)
        {
            var query = $"create database {name}";
            await using DbConnection connection = _dbContext.CreateMasterConnection();
            try
            {
                await connection.ExecuteAsync(query);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(message: "Failed to create database. {ExceptionMessage}", args: exception.Message);
            }
        }
    }
}
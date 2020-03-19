using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brokerage.Common.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brokerage.HostedServices
{
    public class DbSchemaValidationHost : IHostedService
    {
        private readonly ILogger<DbSchemaValidationHost> _logger;
        private readonly DbContextOptionsBuilder<BrokerageContext> _contextOptions;

        public DbSchemaValidationHost(ILogger<DbSchemaValidationHost> logger, DbContextOptionsBuilder<BrokerageContext> contextOptions)
        {
            _logger = logger;
            _contextOptions = contextOptions;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("EF Schema validation is being started...");

            await using var context = new BrokerageContext(_contextOptions.Options);

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

            if (pendingMigrations.Any())
            {
                throw new InvalidOperationException("There are pending migrations, try again later");
            }

            _logger.LogInformation("EF Schema validation has been completed.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

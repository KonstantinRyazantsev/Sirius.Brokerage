using System;
using Brokerage.Common.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Brokerage.Common.Persistence.DesignTime
{
    public class ContextFactory : IDesignTimeDbContextFactory<BrokerageContext>
    {
        public BrokerageContext CreateDbContext(string[] args)
        {
            var connString = Environment.GetEnvironmentVariable("POSTGRE_SQL_CONNECTION_STRING");

            var optionsBuilder = new DbContextOptionsBuilder<BrokerageContext>();
            optionsBuilder.UseNpgsql(connString);

            return new BrokerageContext(optionsBuilder.Options);
        }
    }
}

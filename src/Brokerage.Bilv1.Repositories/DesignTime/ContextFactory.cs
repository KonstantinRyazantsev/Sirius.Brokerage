using System;
using Brokerage.Bilv1.Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Brokerage.Bilv1.Repositories.DesignTime
{
    public class ContextFactory : IDesignTimeDbContextFactory<BrokerageBilV1Context>
    {
        public BrokerageBilV1Context CreateDbContext(string[] args)
        {
            var connString = Environment.GetEnvironmentVariable("POSTGRE_SQL_CONNECTION_STRING");

            var optionsBuilder = new DbContextOptionsBuilder<BrokerageBilV1Context>();
            optionsBuilder.UseNpgsql(connString);

            return new BrokerageBilV1Context(optionsBuilder.Options);
        }
    }
}

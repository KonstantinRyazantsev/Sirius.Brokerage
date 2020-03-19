using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Common.Persistence.DbContexts
{
    public class BrokerageContext : DbContext
    {
        public BrokerageContext(DbContextOptions<BrokerageContext> options) :
            base(options)
        {
        }

        public DbSet<BrokerAccountEntity> BrokerAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(PostgresRepositoryConfiguration.SchemaName);

            modelBuilder.Entity<BrokerAccountEntity>()
                .HasKey(c => new { c.BlockchainId, c.NetworkId, c.BrokerAccountId });

            modelBuilder.Entity<BrokerAccountEntity>()
                .HasIndex(x => x.RequestId)
                .IsUnique(true)
                .HasName("IX_BrokerAccount_RequestId");

            base.OnModelCreating(modelBuilder);
        }
    }
}

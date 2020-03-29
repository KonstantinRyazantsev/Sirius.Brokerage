using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Common.Persistence.DbContexts
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) :
            base(options)
        {
        }

        public DbSet<BrokerAccountEntity> BrokerAccounts { get; set; }

        public DbSet<BrokerAccountRequisitesEntity> BrokerAccountsRequisites { get; set; }

        public DbSet<AccountEntity> Accounts { get; set; }

        public DbSet<AccountRequisitesEntity> AccountRequisites { get; set; }

        public DbSet<BlockchainEntity> Blockchains { get; set; }

        public DbSet<ProtocolEntity> Networks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(PostgresRepositoryConfiguration.SchemaName);


            modelBuilder.Entity<BrokerAccountEntity>()
                .HasKey(c => new { c.BrokerAccountId });

            modelBuilder.Entity<BrokerAccountEntity>()
                .HasIndex(x => x.RequestId)
                .IsUnique(true)
                .HasName("IX_BrokerAccount_RequestId");

            modelBuilder.Entity<BrokerAccountEntity>()
                .Property(b => b.BrokerAccountId)
                .HasIdentityOptions(startValue: 100_000);


            modelBuilder.Entity<ProtocolEntity>()
                .HasKey(c => new { BlockchainId = c.ProtocolId});


            modelBuilder.Entity<BrokerAccountRequisitesEntity>()
                .HasKey(c => new { c.Id });

            modelBuilder.Entity<BrokerAccountRequisitesEntity>()
                .HasIndex(x => x.RequestId)
                .IsUnique(true)
                .HasName("IX_BrokerAccountRequisites_RequestId");

            modelBuilder.Entity<BrokerAccountRequisitesEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: 100_000);

            modelBuilder.Entity<BrokerAccountRequisitesEntity>()
                .HasIndex(x => x.BrokerAccountId)
                .IsUnique(false)
                .HasName("IX_BrokerAccountRequisites_BrokerAccountId"); ;


            modelBuilder.Entity<AccountEntity>()
                .HasKey(c => new { Id = c.AccountId });

            modelBuilder.Entity<AccountEntity>()
                .HasIndex(x => x.RequestId)
                .IsUnique(true)
                .HasName("IX_Account_RequestId");

            modelBuilder.Entity<AccountEntity>()
                .Property(b => b.AccountId)
                .HasIdentityOptions(startValue: 100_000);

            modelBuilder.Entity<BrokerAccountEntity>()
                .HasMany<AccountEntity>(s => s.Accounts)
                .WithOne(s => s.BrokerAccount)
                .HasForeignKey(x => x.BrokerAccountId)
                .IsRequired(true);


            modelBuilder.Entity<AccountRequisitesEntity>()
                .HasKey(c => new { c.Id });

            modelBuilder.Entity<AccountRequisitesEntity>()
                .HasIndex(x => x.RequestId)
                .IsUnique(true)
                .HasName("IX_AccountRequisites_RequestId");

            modelBuilder.Entity<AccountRequisitesEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: 100_000);

            modelBuilder.Entity<AccountEntity>()
                .HasMany<AccountRequisitesEntity>(s => s.AccountRequisites)
                .WithOne(s => s.Account)
                .HasForeignKey(x => x.AccountId);


            base.OnModelCreating(modelBuilder);
        }
    }
}

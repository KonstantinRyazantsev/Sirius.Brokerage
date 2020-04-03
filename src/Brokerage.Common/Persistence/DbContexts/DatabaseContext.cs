using Brokerage.Common.Persistence.Entities;
using Brokerage.Common.ReadModels.Blockchains;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Common.Persistence.DbContexts
{
    public class DatabaseContext : DbContext
    {
        public const string SchemaName = "brokerage";
        public const string MigrationHistoryTable = "__EFMigrationsHistory";

        public DatabaseContext(DbContextOptions<DatabaseContext> options) :
            base(options)
        {
        }

        public DbSet<BrokerAccountEntity> BrokerAccounts { get; set; }
        public DbSet<BrokerAccountRequisitesEntity> BrokerAccountsRequisites { get; set; }

        public DbSet<BrokerAccountBalancesEntity> BrokerAccountBalances { get; set; }

        public DbSet<BrokerAccountBalancesUpdateEntity> BrokerAccountBalancesUpdate { get; set; }

        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<AccountRequisitesEntity> AccountRequisites { get; set; }
        public DbSet<Blockchain> Blockchains { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            BuildBrokerAccount(modelBuilder);
            BuildBrokerAccountRequisites(modelBuilder);
            BuildAccount(modelBuilder);
            BuildAccountRequisites(modelBuilder);
            BuildBlockchain(modelBuilder);
            BuildBrokerAccountBalancesEntity(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void BuildBrokerAccountBalancesEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BrokerAccountBalancesUpdateEntity>()
                .ToTable(Tables.BrokerAccountBalancesUpdate)
                .HasKey(x => x.UpdateId);

            modelBuilder.Entity<BrokerAccountBalancesEntity>()
                .ToTable(Tables.BrokerAccountBalances)
                .HasKey(x => x.BrokerAccountBalancesId);

            modelBuilder.Entity<BrokerAccountBalancesEntity>()
                .HasIndex(x => new
                {
                    x.BrokerAccountId,
                    x.AssetId
                })
                .HasName("IX_BrokerAccountBalances_BrokerAccountId_AssetId");

            modelBuilder.Entity<BrokerAccountBalancesEntity>(e =>
            {
                e.Property(p => p.Version)
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            });
        }

        private static void BuildBlockchain(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blockchain>()
                .ToTable(Tables.Blockchains)
                .HasKey(x => x.BlockchainId);
        }

        private static void BuildAccountRequisites(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountRequisitesEntity>()
                .HasKey(c => new { c.Id });

            modelBuilder.Entity<AccountRequisitesEntity>()
                .HasIndex(x => x.RequestId)
                .IsUnique(true)
                .HasName("IX_AccountRequisites_RequestId");

            modelBuilder.Entity<AccountRequisitesEntity>()
                .HasIndex(x => new
                {
                    x.BlockchainId,
                    x.Address
                })
                .HasName("IX_AccountRequisites_BlockchainId_Address");

            modelBuilder.Entity<AccountRequisitesEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: 100_000);

            modelBuilder.Entity<AccountEntity>()
                .HasMany<AccountRequisitesEntity>(s => s.AccountRequisites)
                .WithOne(s => s.Account)
                .HasForeignKey(x => x.AccountId);
        }

        private static void BuildAccount(ModelBuilder modelBuilder)
        {
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
        }

        private static void BuildBrokerAccountRequisites(ModelBuilder modelBuilder)
        {
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
                .HasName("IX_BrokerAccountRequisites_BrokerAccountId");
            ;
        }

        private static void BuildBrokerAccount(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BrokerAccountEntity>()
                .HasKey(c => new { c.BrokerAccountId });

            modelBuilder.Entity<BrokerAccountEntity>()
                .HasIndex(x => x.RequestId)
                .IsUnique(true)
                .HasName("IX_BrokerAccount_RequestId");

            modelBuilder.Entity<BrokerAccountEntity>()
                .Property(b => b.BrokerAccountId)
                .HasIdentityOptions(startValue: 100_000);
        }
    }
}

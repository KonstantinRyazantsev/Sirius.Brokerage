using Brokerage.Common.Persistence.Entities;
using Brokerage.Common.Persistence.Entities.Deposits;
using Brokerage.Common.Persistence.Entities.Withdrawals;
using Brokerage.Common.ReadModels.Assets;
using Brokerage.Common.ReadModels.Blockchains;
using Microsoft.EntityFrameworkCore;
using Swisschain.Extensions.Idempotency.EfCore;
using DepositSourceEntity = Brokerage.Common.Persistence.Entities.Deposits.DepositSourceEntity;

namespace Brokerage.Common.Persistence.DbContexts
{
    public class DatabaseContext : DbContext, IDbContextWithOutbox
    {
        public const long StartingIdentity = 999_999;
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
        public DbSet<DepositEntity> Deposits { get; set; }
        public DbSet<DepositSourceEntity> DepositSources { get; set; }
        public DbSet<DepositFeeEntity> Fees { get; set; }
        public DbSet<AccountRequisitesEntity> AccountRequisites { get; set; }
        public DbSet<Blockchain> Blockchains { get; set; }
        public DbSet<WithdrawalEntity> Withdrawals { get; set; }
        public DbSet<WithdrawalFeeEntity> WithdrawalFees { get; set; }

        public DbSet<OutboxEntity> Outbox { get; set; }
        public DbSet<Asset> Assets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.BuildOutbox(startAggregateIdFrom: StartingIdentity);

            BuildAssets(modelBuilder);
            BuildBrokerAccount(modelBuilder);
            BuildBrokerAccountRequisites(modelBuilder);
            BuildAccount(modelBuilder);
            BuildAccountRequisites(modelBuilder);
            BuildBlockchain(modelBuilder);
            BuildBrokerAccountBalancesEntity(modelBuilder);
            BuildDepositsEntity(modelBuilder);
            BuildWithdrawalsEntity(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void BuildAssets(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .ToTable(Tables.Assets)
                .HasKey(x => x.Id);
        }

        private static void BuildWithdrawalsEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WithdrawalEntity>()
                .ToTable(Tables.Withdrawals)
                .HasKey(x => x.Id);

            modelBuilder.Entity<WithdrawalFeeEntity>()
                .ToTable(Tables.WithdrawalFees)
                .HasKey(x => new
                {
                    x.WithdrawalId,
                    x.AssetId
                });

            modelBuilder.Entity<WithdrawalEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: StartingIdentity);

            modelBuilder.Entity<WithdrawalEntity>(e =>
            {
                e.Property(p => p.Version)
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<WithdrawalEntity>()
                .HasIndex(x => x.TransactionId)
                .HasName("IX_Withdrawal_TransactionId");

            modelBuilder.Entity<WithdrawalEntity>()
                .HasIndex(x => x.WithdrawalOperationId)
                .HasName("IX_Withdrawal_OperationId");


            modelBuilder.Entity<WithdrawalEntity>()
                .HasMany(x => x.Fees)
                .WithOne(x => x.WithdrawalEntity)
                .HasForeignKey(x => x.WithdrawalId);
        }

        private static void BuildDepositsEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepositEntity>()
                .ToTable(Tables.Deposits)
                .HasKey(x => x.Id);

            modelBuilder.Entity<DepositSourceEntity>()
                .ToTable(Tables.DepositSources)
                .HasKey(x => new
                {
                     x.DepositId,
                     x.Address
                });

            modelBuilder.Entity<DepositFeeEntity>()
                .ToTable(Tables.DepositFees)
                .HasKey(x => new
                {
                    x.DepositId,
                    x.AssetId
                });

            modelBuilder.Entity<DepositEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: StartingIdentity);

            modelBuilder.Entity<DepositEntity>(e =>
            {
                e.Property(p => p.Version)
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<DepositEntity>()
                .HasIndex(x => new
                {
                    x.TransactionId,
                    x.AssetId,
                    x.BrokerAccountRequisitesId,
                    x.AccountRequisitesId
                })
                .IsUnique(true)
                .HasName("IX_Deposit_NaturalId");

            modelBuilder.Entity<DepositEntity>()
                .HasIndex(x => x.TransactionId)
                .HasName("IX_Deposit_TransactionId");

            modelBuilder.Entity<DepositEntity>()
                .HasIndex(x => x.ConsolidationOperationId)
                .HasName("IX_Deposit_ConsolidationOperationId");


            modelBuilder.Entity<DepositEntity>()
                .HasMany(x => x.Fees)
                .WithOne(x => x.DepositEntity)
                .HasForeignKey(x => x.DepositId);

            modelBuilder.Entity<DepositEntity>()
                .HasMany(x => x.Sources)
                .WithOne(x => x.DepositEntity)
                .HasForeignKey(x => x.DepositId);
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
                .IsUnique()
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
                .HasIdentityOptions(startValue: StartingIdentity);

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
                .HasIdentityOptions(startValue: StartingIdentity);

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
                .HasIdentityOptions(startValue: StartingIdentity);

            modelBuilder.Entity<BrokerAccountRequisitesEntity>()
                .HasIndex(x => x.BrokerAccountId)
                .IsUnique(false)
                .HasName("IX_BrokerAccountRequisites_BrokerAccountId");

            modelBuilder.Entity<BrokerAccountRequisitesEntity>()
                .HasIndex(x => x.BlockchainId)
                .IsUnique(false)
                .HasName("IX_BrokerAccountRequisites_BlockchainId");
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
                .HasIdentityOptions(startValue: StartingIdentity);
        }
    }
}

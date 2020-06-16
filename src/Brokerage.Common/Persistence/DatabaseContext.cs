using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Persistence.Transactions;
using Brokerage.Common.Persistence.Withdrawals;
using Brokerage.Common.ReadModels.Assets;
using Brokerage.Common.ReadModels.Blockchains;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Swisschain.Extensions.Idempotency.EfCore;
using DepositSourceEntity = Brokerage.Common.Persistence.Deposits.DepositSourceEntity;

namespace Brokerage.Common.Persistence
{
    public class DatabaseContext : DbContext, IDbContextWithOutbox
    {
        public const string SchemaName = "brokerage";
        public const string MigrationHistoryTable = "__EFMigrationsHistory";

        public DatabaseContext(DbContextOptions<DatabaseContext> options) :
            base(options)
        {
        }

        public DbSet<OutboxEntity> Outbox { get; set; }
        public DbSet<BrokerAccountEntity> BrokerAccounts { get; set; }
        public DbSet<BrokerAccountDetailsEntity> BrokerAccountsDetails { get; set; }
        public DbSet<BrokerAccountBalancesEntity> BrokerAccountBalances { get; set; }
        public DbSet<BrokerAccountBalancesUpdateEntity> BrokerAccountBalancesUpdate { get; set; }
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<DepositEntity> Deposits { get; set; }
        public DbSet<DepositSourceEntity> DepositSources { get; set; }
        public DbSet<DepositFeeEntity> Fees { get; set; }
        public DbSet<AccountDetailsEntity> AccountDetails { get; set; }
        public DbSet<Blockchain> Blockchains { get; set; }
        public DbSet<WithdrawalEntity> Withdrawals { get; set; }
        public DbSet<WithdrawalFeeEntity> WithdrawalFees { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<OperationEntity> Operations { get; set; }
        public DbSet<DetectedTransactionEntity> DetectedTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.BuildOutbox();

            BuildAssets(modelBuilder);
            BuildBlockchains(modelBuilder);

            BuildBrokerAccounts(modelBuilder);
            BuildBrokerAccountDetails(modelBuilder);
            BuildBrokerAccountBalances(modelBuilder);

            BuildAccounts(modelBuilder);
            BuildAccountDetails(modelBuilder);

            BuildDeposits(modelBuilder);
            BuildWithdrawals(modelBuilder);

            BuildOperations(modelBuilder);
            BuildDetectedTransactions(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void BuildDetectedTransactions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetectedTransactionEntity>()
                .ToTable(Tables.DetectedTransactions)
                .HasKey(x => new
                {
                    x.BlockchainId,
                    x.TransactionId
                });
        }

        private static void BuildOperations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OperationEntity>()
                .ToTable(Tables.Operations)
                .HasKey(x => x.Id);
        }

        private static void BuildAssets(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .ToTable(Tables.Assets)
                .HasKey(x => x.Id);
        }

        private static void BuildWithdrawals(ModelBuilder modelBuilder)
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
                .HasIdentityOptions(startValue: 10600000);

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
                .HasIndex(x => x.OperationId)
                .HasName("IX_Withdrawal_OperationId");


            modelBuilder.Entity<WithdrawalEntity>()
                .HasMany(x => x.Fees)
                .WithOne(x => x.WithdrawalEntity)
                .HasForeignKey(x => x.WithdrawalId);
        }

        private static void BuildDeposits(ModelBuilder modelBuilder)
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
                .HasIdentityOptions(startValue: 10500000);

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
                    x.BlockchainId,
                    x.TransactionId
                })
                .HasName("IX_Deposit_BlockchainId_TransactionId");

            modelBuilder.Entity<DepositEntity>()
                .HasIndex(x => x.ConsolidationOperationId)
                .IsUnique()
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

        private static void BuildBrokerAccountBalances(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BrokerAccountBalancesUpdateEntity>()
                .ToTable(Tables.BrokerAccountBalancesUpdate)
                .HasKey(x => x.UpdateId);

            modelBuilder.Entity<BrokerAccountBalancesEntity>()
                .ToTable(Tables.BrokerAccountBalances)
                .HasKey(x => x.Id);

            modelBuilder.Entity<BrokerAccountBalancesEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: 10400000);

            modelBuilder.Entity<BrokerAccountBalancesEntity>()
                .HasIndex(x => x.NaturalId)
                .IsUnique()
                .HasName("IX_BrokerAccountBalances_NaturalId");

            modelBuilder.Entity<BrokerAccountBalancesEntity>(e =>
            {
                e.Property(p => p.Version)
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            });
        }

        private static void BuildBlockchains(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blockchain>()
                .ToTable(Tables.Blockchains)
                .HasKey(x => x.Id);

            modelBuilder.Entity<Blockchain>()
                .OwnsOne<Protocol>(x => x.Protocol,
                    c =>
                    {
                        c.Property(p => p.Code).HasColumnName("ProtocolCode");
                        c.Property(p => p.Name).HasColumnName("ProtocolName");
                        c.Property(p => p.StartBlockNumber).HasColumnName("StartBlockNumber");
                        c.Property(p => p.DoubleSpendingProtectionType).HasColumnName("DoubleSpendingProtectionType");

                            // TODO: Include BlockchainID to the index
                            c.HasIndex(x => x.Code).HasName("IX_Blockchain_ProtocolCode");
                        c.HasIndex(x => x.Name).HasName("IX_Blockchain_ProtocolName");

                        c.OwnsOne<Requirements>(x => x.Requirements);
                        c.OwnsOne<Capabilities>(x => x.Capabilities,
                            z =>
                            {
                                z.OwnsOne(x => x.DestinationTag,
                                    y =>
                                    {
                                        y.OwnsOne(x => x.Text);
                                        y.OwnsOne(x => x.Number);
                                    });
                            });
                    });

            modelBuilder.Entity<Blockchain>()
                .HasIndex(x => x.Name)
                .HasName("IX_Blockchain_Name");

            modelBuilder.Entity<Blockchain>()
                .HasIndex(x => x.NetworkType)
                .HasName("IX_Blockchain_NetworkType");

            modelBuilder.Entity<Blockchain>()
                .HasIndex(x => x.TenantId)
                .HasName("IX_Blockchain_TenantId");

            modelBuilder.Entity<Blockchain>()
                .HasIndex(x => x.ChainSequence)
                .HasName("IX_Blockchain_ChainSequence");

            modelBuilder.Entity<Blockchain>()
                .Property(x => x.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }

        private static void BuildAccountDetails(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountDetailsEntity>()
                .ToTable(Tables.AccountDetails)
                .HasKey(c => new { c.Id });

            modelBuilder.Entity<AccountDetailsEntity>()
                .HasIndex(x => x.NaturalId)
                .HasName("IX_AccountDetails_NaturalId")
                .IsUnique(true);

            modelBuilder.Entity<AccountDetailsEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: 10300000);

            modelBuilder.Entity<AccountDetailsEntity>()
                .HasIndex(x => new
                {
                    x.AccountId,
                    x.BlockchainId
                })
                .IsUnique()
                .HasName("IX_AccountDetails_AccountId_BlockchainId");

            modelBuilder.Entity<AccountEntity>()
                .HasMany<AccountDetailsEntity>(s => s.AccountDetails)
                .WithOne(s => s.Account)
                .HasForeignKey(x => x.AccountId);
        }

        private static void BuildAccounts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountEntity>()
                .HasKey(c => new { Id = c.Id });

            modelBuilder.Entity<AccountEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: 10200000);

            modelBuilder.Entity<BrokerAccountEntity>()
                .HasMany<AccountEntity>(s => s.Accounts)
                .WithOne(s => s.BrokerAccount)
                .HasForeignKey(x => x.BrokerAccountId)
                .IsRequired(true);
        }

        private static void BuildBrokerAccountDetails(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BrokerAccountDetailsEntity>()
                .HasKey(c => new { c.Id });

            modelBuilder.Entity<BrokerAccountDetailsEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: 10100000);

            modelBuilder.Entity<BrokerAccountDetailsEntity>()
                .HasIndex(x => x.ActiveId)
                .HasName("IX_BrokerAccountDetails_ActiveId");

            modelBuilder.Entity<BrokerAccountDetailsEntity>()
                .HasIndex(x => x.NaturalId)
                .IsUnique()
                .HasName("IX_BrokerAccountDetails_NaturalId");

            modelBuilder.Entity<BrokerAccountDetailsEntity>()
                .HasIndex(x => x.BrokerAccountId)
                .HasName("IX_BrokerAccountDetails_BrokerAccountId");

            modelBuilder.Entity<BrokerAccountDetailsEntity>()
                .HasIndex(x => x.Id)
                .HasSortOrder(SortOrder.Descending)
                .HasName("IX_BrokerAccountDetails_IdDesc");
        }

        private static void BuildBrokerAccounts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BrokerAccountEntity>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<BrokerAccountEntity>()
                .HasIndex(x => x.RequestId)
                .IsUnique(true)
                .HasName("IX_BrokerAccount_RequestId");

            modelBuilder.Entity<BrokerAccountEntity>()
                .Property(b => b.Id)
                .HasIdentityOptions(startValue: 10000000);
        }
    }
}

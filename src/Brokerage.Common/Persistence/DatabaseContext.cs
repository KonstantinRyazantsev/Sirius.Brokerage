using System.Collections.Generic;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccounts;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Persistence.Transactions;
using Brokerage.Common.Persistence.Withdrawals;
using Brokerage.Common.ReadModels.Assets;
using Brokerage.Common.ReadModels.Blockchains;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Swisschain.Extensions.Idempotency.EfCore;
using DepositSourceEntity = Brokerage.Common.Persistence.Deposits.DepositSourceEntity;

namespace Brokerage.Common.Persistence
{
    public class DatabaseContext : DbContext, IDbContextWithOutbox, IDbContextWithIdGenerator
    {
        public const string SchemaName = "brokerage";
        public const string MigrationHistoryTable = "__EFMigrationsHistory";
        private static readonly JsonSerializerSettings JsonSerializingSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public DatabaseContext(DbContextOptions<DatabaseContext> options) :
            base(options)
        {
        }

        public DbSet<OutboxEntity> Outbox { get; set; }
        public DbSet<IdGeneratorEntity> IsGenerator { get; set; }
        public DbSet<BrokerAccountEntity> BrokerAccounts { get; set; }
        public DbSet<BrokerAccountDetailsEntity> BrokerAccountsDetails { get; set; }
        public DbSet<BrokerAccountBalancesEntity> BrokerAccountBalances { get; set; }
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<DepositEntity> Deposits { get; set; }
        public DbSet<AccountDetailsEntity> AccountDetails { get; set; }
        public DbSet<Blockchain> Blockchains { get; set; }
        public DbSet<WithdrawalEntity> Withdrawals { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<OperationEntity> Operations { get; set; }
        public DbSet<DetectedTransactionEntity> DetectedTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.BuildIdempotency(x =>
            {
                x.AddIdGenerator(IdGenerators.BrokerAccounts, 100000000);
                x.AddIdGenerator(IdGenerators.BrokerAccountDetails, 101000000);
                x.AddIdGenerator(IdGenerators.BrokerAccountsBalances, 102000000);
                x.AddIdGenerator(IdGenerators.Accounts, 103000000);
                x.AddIdGenerator(IdGenerators.AccountDetails, 104000000);
                x.AddIdGenerator(IdGenerators.Deposits, 105000000);
                x.AddIdGenerator(IdGenerators.Withdrawals, 106000000);
            });

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

            modelBuilder.Entity<OperationEntity>(e =>
            {
                e.Property(p => p.Version)
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            });

            #region Conversions

            modelBuilder.Entity<OperationEntity>()
                .Property(e => e.ExpectedFees)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<IReadOnlyCollection<ExpectedOperationFeeEntity>>(v,
                            JsonSerializingSettings));

            modelBuilder.Entity<OperationEntity>()
                .Property(e => e.ActualFees)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<IReadOnlyCollection<ActualOperationFeeEntity>>(v,
                            JsonSerializingSettings));

            #endregion

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

            #region Conversions

            modelBuilder.Entity<WithdrawalEntity>()
                .Property(e => e.Fees)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<IReadOnlyCollection<WithdrawalFeeEntity>>(v,
                            JsonSerializingSettings));

            #endregion
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

            modelBuilder.Entity<DepositEntity>(e =>
            {
                e.Property(p => p.Version)
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<DepositEntity>()
                .HasIndex(x => x.BlockchainId)
                .HasName("IX_Deposit_BlockchainId");

            modelBuilder.Entity<DepositEntity>()
                .HasIndex(x => x.TransactionId)
                .HasName("IX_Deposit_TransactionId");

            modelBuilder.Entity<DepositEntity>()
                .HasIndex(x => x.ConsolidationOperationId)
                .HasName("IX_Deposit_ConsolidationOperationId");


            #region Conversions

            modelBuilder.Entity<DepositEntity>()
                .Property(e => e.Fees)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<IReadOnlyCollection<DepositFeeEntity>>(v,
                            JsonSerializingSettings));

            modelBuilder.Entity<DepositEntity>()
                .Property(e => e.Sources)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v,
                        JsonSerializingSettings),
                    v =>
                        JsonConvert.DeserializeObject<IReadOnlyCollection<DepositSourceEntity>>(v,
                            JsonSerializingSettings));

            #endregion
        }

        private static void BuildBrokerAccountBalances(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BrokerAccountBalancesEntity>()
                .ToTable(Tables.BrokerAccountBalances)
                .HasKey(x => x.Id);

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

            
            var jsonSerializingSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            modelBuilder.Entity<Blockchain>()
                .Property(e => e.Protocol)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, jsonSerializingSettings),
                    v => JsonConvert.DeserializeObject<Protocol>(v, jsonSerializingSettings));
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

        }
    }
}

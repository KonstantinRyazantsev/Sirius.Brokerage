﻿using Brokerage.Common.Persistence.Entities;
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

            base.OnModelCreating(modelBuilder);
        }
    }
}

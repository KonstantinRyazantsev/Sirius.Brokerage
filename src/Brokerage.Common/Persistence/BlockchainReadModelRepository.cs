﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Blockchains;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public class BlockchainReadModelRepository : IBlockchainReadModelRepository
    {
        private readonly DbContextOptionsBuilder<BrokerageContext> _dbContextOptionsBuilder;

        public BlockchainReadModelRepository(DbContextOptionsBuilder<BrokerageContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }


        public async Task<Blockchain> GetOrDefaultAsync(BlockchainId blockchainId)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var entity = await context.Blockchains.FindAsync(blockchainId.Value);

            return MapToDomain(entity);
        }

        public async Task<IReadOnlyCollection<Blockchain>> GetManyAsync(BlockchainId cursor, int take)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var query = context.Blockchains.Select(x => x);

            if (cursor != null)
            {
                query = query.Where(x => x.BlockchainId.CompareTo(cursor.Value) > 1);
            }

            var result = await query
                .OrderBy(x => x.BlockchainId)
                .Take(take)
                .ToListAsync();

            return result.Select(MapToDomain).ToArray();
        }

        public async Task<Blockchain> AddOrReplaceAsync(Blockchain blockchain)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);
            var entity = new BlockchainEntity() { BlockchainId = blockchain.BlockchainId };

            try
            {
                context.Blockchains.Add(entity);
                await context.SaveChangesAsync();

                return MapToDomain(entity);
            }
            catch (DbUpdateException e) //Check that request was already processed (by constraint)
                when (e.InnerException is PostgresException pgEx &&
                      pgEx.SqlState == "23505")
            {
                context.Blockchains.Update(entity);

                await context.SaveChangesAsync();

                return MapToDomain(entity);
            }
        }

        private static Blockchain MapToDomain(BlockchainEntity blockchainEntity)
        {
            return new Blockchain() {BlockchainId = blockchainEntity.BlockchainId};
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.Wallets;
using Brokerage.Bilv1.Domain.Repositories;
using Brokerage.Bilv1.Repositories.DbContexts;
using Brokerage.Bilv1.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Bilv1.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly DbContextOptionsBuilder<BrokerageBilV1Context> _dbContextOptionsBuilder;

        public WalletRepository(DbContextOptionsBuilder<BrokerageBilV1Context> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task AddWalletAsync(string blockchainId, string networkId, Wallet wallet)
        {
            using (var context = new BrokerageBilV1Context(_dbContextOptionsBuilder.Options))
            {
                var existing = await context
                    .Wallets
                    .Where(x => x.BlockchainId == blockchainId &&
                                          x.NetworkId == networkId &&
                                          x.WalletAddress == wallet.Address.ToLowerInvariant())
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    var walletEntity = MapToEntity(blockchainId, networkId, wallet);

                    context.Wallets.Add(walletEntity);

                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<Wallet> GetAsync(string blockchainId, string networkId, string walletAddress)
        {
            using (var context = new BrokerageBilV1Context(_dbContextOptionsBuilder.Options))
            {
                var existing = await context
                    .Wallets
                    .Where(x => x.BlockchainId == blockchainId &&
                                x.NetworkId == networkId &&
                                x.WalletAddress == walletAddress.ToLowerInvariant())
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    return MapFromEntity(existing);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<Wallet> GetAsync(string blockchainId, string networkId, Guid walletId)
        {
            using (var context = new BrokerageBilV1Context(_dbContextOptionsBuilder.Options))
            {
                var existing = await context.Wallets.FindAsync(blockchainId, networkId, walletId);

                if (existing != null)
                {
                    return MapFromEntity(existing);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<IReadOnlyCollection<Wallet>> GetManyAsync(string blockchainId, string networkId, int skip, int take = 100)
        {
            using (var context = new BrokerageBilV1Context(_dbContextOptionsBuilder.Options))
            {
                var wallets = context.Wallets
                    .Where(x => x.BlockchainId == blockchainId && x.NetworkId == networkId)
                    .OrderBy(x => x.BlockchainId)
                    .ThenBy(x => x.NetworkId)
                    .ThenBy(x => x.Id)
                    .Skip(skip)
                    .Take(take);

                await wallets.LoadAsync();

                return wallets.Select(MapFromEntity).ToImmutableArray();
            }
        }

        public async Task DeleteAsync(string blockchainId, string networkId, Guid walletId)
        {
            using (var context = new BrokerageBilV1Context(_dbContextOptionsBuilder.Options))
            {
                var result = await context.Wallets.FindAsync(
                    blockchainId, networkId, walletId);

                if (result != null)
                    context.Wallets.Remove(result);

                await context.SaveChangesAsync();
            }
        }

        private static Wallet MapFromEntity(WalletEntity entity)
        {
            if (entity == null)
                return null;

            return new Wallet()
            {
                Id = entity.Id,
                PubKey = entity.PublicKey,
                Address = entity.OriginalWalletAddress,
                ImportedDateTime = entity.ImportedDateTime,
                TransferCallbackOptions = new TransferCallbackOptions(),
                IsCompromised = entity.IsCompromised
            };
        }

        private static WalletEntity MapToEntity(string blockchainId, string networkId, Wallet wallet)
        {
            return new WalletEntity()
            {
                Id = wallet.Id,
                WalletAddress = wallet.Address.ToLowerInvariant(),
                PublicKey = wallet.PubKey,
                BlockchainId = blockchainId,
                ImportedDateTime = wallet.ImportedDateTime.ToUniversalTime(),
                IsCompromised = wallet.IsCompromised,
                OriginalWalletAddress = wallet.Address,
                NetworkId = networkId
            };
        }
    }
}

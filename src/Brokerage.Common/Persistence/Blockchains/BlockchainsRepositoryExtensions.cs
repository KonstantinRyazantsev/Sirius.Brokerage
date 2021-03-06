﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Persistence.Blockchains
{
    public static class BlockchainsRepositoryExtensions
    {
        public static async Task<IReadOnlyCollection<Blockchain>> GetAllAsync(this IBlockchainsRepository repository)
        {
            var cursor = default(string);
            var result = new List<Blockchain>();

            do
            {
                var page = await repository.GetAllAsync(cursor, 100);

                if (!page.Any())
                {
                    break;
                }

                cursor = page.Last().Id;

                result.AddRange(page);
            } while (true);

            return result;
        }
    }
}

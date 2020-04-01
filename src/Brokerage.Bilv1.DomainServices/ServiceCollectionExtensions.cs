﻿using System;
using System.Collections.Generic;
using Brokerage.Bilv1.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brokerage.Bilv1.DomainServices
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBilV1Services(this IServiceCollection services)
        {
            services.AddTransient<IAssetService, AssetService>();
            services.AddTransient<IWalletsService, WalletsService>();

            services.AddSingleton<BlockchainApiClientProvider>(c => new BlockchainApiClientProvider(
                c.GetRequiredService<ILoggerFactory>(),
                IBlockchainsRepository));

            return services;
        }
    }
}

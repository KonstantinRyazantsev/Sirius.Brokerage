﻿using Brokerage.Bilv1.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Bilv1.Repositories
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBilV1Repositories(this IServiceCollection services)
        {
            services.AddTransient<IEnrolledBalanceRepository, EnrolledBalanceRepository>();
            services.AddTransient<IOperationRepository, OperationRepository>();

            return services;
        }
    }
}
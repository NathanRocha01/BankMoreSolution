using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transferencia.Domain.Interfaces;
using Transferencia.Infrastructure.Context;
using Transferencia.Infrastructure.Repositories;

namespace Transferencia.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DapperContext>();

            // Repositórios
            services.AddScoped<ITransferenciaLogRepository, TransferenciaLogRepository>();
            services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();
            services.AddHttpClient<ITransferenciaRepository, TransferenciaRepository>(client =>
            {
                var baseUrl = configuration["Services:ContaCorrenteApi"];
                client.BaseAddress = new Uri(baseUrl!);
            });

            return services;
        }
    }
}

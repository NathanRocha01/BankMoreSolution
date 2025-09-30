using ContaCorrente.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Tests.Factorys
{
    public class ContaWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
    {
        private string _dbPath;
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // cria caminho único para o arquivo SQLite temporário
                _dbPath = Path.Combine(Path.GetTempPath(), $"contadb_{Guid.NewGuid()}.db");

                var settings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", $"DataSource={_dbPath}" }
            };

                config.AddInMemoryCollection(settings!);
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDistributedCache));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDistributedMemoryCache();
                // substitui DapperContext para usar o arquivo físico
                services.AddScoped<DapperContext>(_ =>
                {
                    var cfg = new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string?>
                        {
                        { "ConnectionStrings:DefaultConnection", $"DataSource={_dbPath}" }
                        })
                        .Build();

                    return new DapperContext(cfg);
                });

                // cria schema no arquivo físico
                using var connection = new SqliteConnection($"DataSource={_dbPath}");
                connection.Open();

                // cria schema inicial
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    
                DROP TABLE IF EXISTS contacorrente;
                DROP TABLE IF EXISTS movimento;
                DROP TABLE IF EXISTS idempotencia;

                CREATE TABLE IF NOT EXISTS contacorrente (
                    idcontacorrente TEXT(37) PRIMARY KEY,
                    numero INTEGER(10) NOT NULL UNIQUE,
                    nome TEXT(100) NOT NULL,
                    cpf TEXT(14) NOT NULL UNIQUE,
                    ativo INTEGER(1) NOT NULL default 0 CHECK (ativo IN (0, 1)),
                    senha TEXT(100) NOT NULL,
                    salt TEXT(100) NOT NULL
                );

                CREATE TABLE IF NOT EXISTS movimento (
                    idmovimento TEXT(37) PRIMARY KEY,
                    idcontacorrente TEXT(37) NOT NULL,
                    datamovimento TEXT(25) NOT NULL,
                    tipomovimento TEXT(1) NOT NULL CHECK (tipomovimento IN ('C','D')),
                    valor REAL NOT NULL,
                    FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente)
                );

                CREATE TABLE IF NOT EXISTS idempotencia (
                    chave_idempotencia TEXT(37) PRIMARY KEY,
                    requisicao TEXT(1000),
                    resultado TEXT(1000)
                );
            ";
                command.ExecuteNonQuery();
            });

            return base.CreateHost(builder);
        }
    }
}

using Microsoft.Data.Sqlite;
using Tarifa.Worker.Domain.Entities;
using Dapper;

namespace Tarifa.Worker.Infrastructure
{
    public interface ITarifaRepository
    {
        Task AddAsync(TarifaE tarifa);
    }

    public class TarifaRepository : ITarifaRepository
    {
        private readonly string _connectionString;

        public TarifaRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task AddAsync(TarifaE tarifa)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.ExecuteAsync(
                "INSERT INTO tarifa (idtarifa, idcontacorrente, datamovimento, valor) VALUES (@IdTarifa, @IdContaCorrente, @DataMovimento, @Valor)",
                tarifa
            );
        }
    }
}

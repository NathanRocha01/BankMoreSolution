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
        private readonly DapperContext _context;

        public TarifaRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TarifaE tarifa)
        {
            using var conn = _context.CreateConnection();
            await conn.ExecuteAsync(
                "INSERT INTO tarifa (idtarifa, idcontacorrente, datamovimento, valor) VALUES (@IdTarifa, @IdContaCorrente, @DataMovimento, @Valor)",
                tarifa
            );
        }
    }
}

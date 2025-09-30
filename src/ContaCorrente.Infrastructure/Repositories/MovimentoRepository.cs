using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
using ContaCorrente.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ContaCorrente.Infrastructure.Repositories
{
    public class MovimentoRepository : IMovimentoRepository
    {
        private readonly DapperContext _context;

        public MovimentoRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movimento>> ObterPorContaAsync(string idContaCorrente)
        {
            const string sql = @"SELECT * FROM movimento WHERE idcontacorrente = @IdContaCorrente ORDER BY datamovimento DESC";

            using var conn = _context.CreateConnection();
            return await conn.QueryAsync<Movimento>(sql, new { IdContaCorrente = idContaCorrente });
        }

        public async Task CriarAsync(Movimento movimento)
        {
            const string sql = @"
            INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
            VALUES (@IdMovimento, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)";

            using var conn = _context.CreateConnection();
            await conn.ExecuteAsync(sql, movimento);
        }
    }
}

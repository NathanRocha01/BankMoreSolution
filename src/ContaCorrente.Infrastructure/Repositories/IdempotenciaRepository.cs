using Shared.Interface;
using Shared.Entities;
using ContaCorrente.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ContaCorrente.Infrastructure.Repositories
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly DapperContext _context;

        public IdempotenciaRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Idempotencia?> ObterAsync(string chaveIdempotencia)
        {
            const string sql = "SELECT * FROM idempotencia WHERE chave_idempotencia = @ChaveIdempotencia";
            using var conn = _context.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Idempotencia>(sql, new { ChaveIdempotencia = chaveIdempotencia });
        }

        public async Task CriarAsync(Idempotencia registro)
        {
            const string sql = @"
            INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
            VALUES (@ChaveIdempotencia, @Requisicao, @Resultado)";
            using var conn = _context.CreateConnection();
            await conn.ExecuteAsync(sql, registro);
        }
    }
}

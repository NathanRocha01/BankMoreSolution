using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transferencia.Domain.Entities;
using Transferencia.Domain.Interfaces;
using Transferencia.Infrastructure.Context;

namespace Transferencia.Infrastructure.Repositories
{
    public class TransferenciaLogRepository : ITransferenciaLogRepository
    {
        private readonly DapperContext _context;

        public TransferenciaLogRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task CriarAsync(TransferenciaEntity transferencia)
        {
            const string sql = @"INSERT INTO transferencia 
                (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor)
                VALUES (@IdTransferencia, @IdContaCorrenteOrigem, @IdContaCorrenteDestino, @DataMovimento, @Valor)";

            using var conn = _context.CreateConnection();
            await conn.ExecuteAsync(sql, transferencia);
        }
    }
}

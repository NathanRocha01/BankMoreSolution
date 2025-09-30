using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
using ContaCorrente.Infrastructure.Context;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Infrastructure.Repositories
{
    public class ContaRepository : IContaRepository
    {
        private readonly DapperContext _context;

        public ContaRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Conta?> ObterPorNumeroAsync(int numero)
        {
            const string sql = "SELECT * FROM contacorrente WHERE numero = @Numero";
            using var conn = _context.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Conta>(sql, new { Numero = numero });
        }
        public async Task<Conta?> ObterPorCpfAsync(string cpf)
        {
            const string sql = @"SELECT * FROM ContaCorrente WHERE Cpf = @Cpf LIMIT 1";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Conta>(sql, new { Cpf = cpf });
        }

        public async Task<Conta?> ObterPorIdAsync(string id)
        {
            const string sql = @"SELECT * FROM ContaCorrente WHERE idcontacorrente = @Id LIMIT 1";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Conta>(sql, new { Id = id });
        }

        public async Task<bool> VerificarExistenciaNumeroAsync(int numero)
        {
            const string sql = "SELECT COUNT(1) FROM contacorrente WHERE numero = @Numero";
            using var conn = _context.CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(sql, new { Numero = numero });
            return count > 0;
        }

        public async Task CriarAsync(Conta conta)
        {
            const string sql = @"
            INSERT INTO contacorrente (idcontacorrente, numero, nome, cpf, ativo, senha, salt)
            VALUES (@IdContaCorrente, @Numero, @Nome, @Cpf, @Ativo, @Senha, @Salt)";

            using var conn = _context.CreateConnection();
            await conn.ExecuteAsync(sql, conta);
        }

        public async Task AtualizarAsync(Conta conta)
        {
            const string sql = @"
            UPDATE contacorrente SET
                nome = @Nome,
                ativo = @Ativo,
                senha = @Senha,
                salt = @Salt
            WHERE idcontacorrente = @IdContaCorrente";

            using var conn = _context.CreateConnection();
            await conn.ExecuteAsync(sql, conta);
        }
    }
}

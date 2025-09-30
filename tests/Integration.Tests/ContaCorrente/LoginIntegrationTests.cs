using ContaCorrente.Application.DTOs;
using FluentAssertions;
using Integration.Tests.Factorys;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Integration.Tests.ContaCorrente
{
    public class LoginIntegrationTests : IntegrationTestBase<ProgramC, ContaWebApplicationFactory<ProgramC>>
    {
        public LoginIntegrationTests(ContaWebApplicationFactory<ProgramC> factory)
            : base(factory) { }

        [Fact]
        public async Task Deve_FazerLogin_QuandoCredenciaisValidas()
        {
            var cpf = CpfGenerator.GerarCpfValido();
            // 1. Criar conta primeiro
            var criarConta = new
            {
                Nome = "Fulano",
                Cpf = cpf,
                Senha = "123456"
            };
            var criarResp = await _client.PostAsJsonAsync("/api/conta", criarConta);
            criarResp.EnsureSuccessStatusCode();

            // 2. Fazer login
            var loginBody = new
            {
                Cpf = cpf,
                Senha = "123456"
            };
            var loginResp = await _client.PostAsJsonAsync("/api/login", loginBody);

            loginResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
            string token = result.GetProperty("token").GetString()!;

            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Deve_RetornarUnauthorized_QuandoSenhaInvalida()
        {
            var cpf = CpfGenerator.GerarCpfValido();
            var criarConta = new
            {
                Nome = "Beltrano",
                Cpf = cpf,
                Senha = "654321"
            };
            var criarResp = await _client.PostAsJsonAsync("/api/conta", criarConta);
            criarResp.EnsureSuccessStatusCode();

            var loginBody = new
            {
                Cpf = cpf,
                Senha = "errada"
            };
            var loginResp = await _client.PostAsJsonAsync("/api/login", loginBody);

            loginResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var result = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
            string type = result.GetProperty("type").GetString()!;
            string message = result.GetProperty("message").GetString()!;

            type.Should().Be("USER_UNAUTHORIZED");
            message.Should().Contain("Senha inválida");
        }

        [Fact]
        public async Task Deve_ConsultarSaldo_AposLogin()
        {
            var cpf = CpfGenerator.GerarCpfValido();
            // 1. Criar conta
            var criarConta = new
            {
                Nome = "Ciclano",
                Cpf = cpf,
                Senha = "senha123"
            };
            var criarResp = await _client.PostAsJsonAsync("/api/conta", criarConta);
            criarResp.EnsureSuccessStatusCode();

            // 2. Login
            var loginBody = new
            {
                Cpf = cpf,
                Senha = "senha123"
            };
            var loginResp = await _client.PostAsJsonAsync("/api/login", loginBody);
            loginResp.EnsureSuccessStatusCode();

            var tokenObj = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
            string token = tokenObj.GetProperty("token").GetString()!;

            // 3. Chamar saldo autenticado
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var saldoResp = await _client.GetAsync("/api/conta/saldo");
            saldoResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var saldo = await saldoResp.Content.ReadFromJsonAsync<SaldoResponse>();
            saldo.Should().NotBeNull();
            saldo.NumeroConta.Should().BeGreaterThan(0);
            saldo.Nome.Should().Be("Ciclano");
        }
    }
}

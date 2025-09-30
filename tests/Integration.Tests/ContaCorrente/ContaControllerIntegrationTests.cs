using FluentAssertions;
using Integration.Tests.Factorys;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Integration.Tests.ContaCorrente
{
    public class ContaControllerIntegrationTests : IntegrationTestBase<ProgramC, ContaWebApplicationFactory<ProgramC>>
    {
        public ContaControllerIntegrationTests(ContaWebApplicationFactory<ProgramC> factory)
            : base(factory) { }

        [Fact]
        public async Task Deve_CriarConta_QuandoDadosValidos()
        {
            var body = new
            {
                Nome = "Fulano",
                Cpf = CpfGenerator.GerarCpfValido(),
                Senha = "123456"
            };

            var response = await _client.PostAsJsonAsync("/api/conta", body);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            int numeroConta = result.GetProperty("numeroConta").GetInt32();
            numeroConta.Should().NotBe(0);
        }

        [Fact]
        public async Task Deve_RetornarErro_QuandoCpfInvalido()
        {
            var body = new
            {
                Nome = "Fulano",
                Cpf = "00000000000", // inválido
                Senha = "123456"
            };

            var response = await _client.PostAsJsonAsync("/api/conta", body);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();

            string type = result.GetProperty("type").GetString()!;
            string message = result.GetProperty("message").GetString()!;

            type.Should().Be("INVALID_DOCUMENT");
            message.Should().Contain("CPF inválido");
        }

        [Fact]
        public async Task Deve_RetornarUnauthorized_QuandoInativarSemToken()
        {
            var body = new { Senha = "123456" };

            var response = await _client.PatchAsJsonAsync("/api/conta/inativar", body);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Deve_RetornarUnauthorized_QuandoConsultarSaldoSemToken()
        {
            var response = await _client.GetAsync("/api/conta/saldo");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}

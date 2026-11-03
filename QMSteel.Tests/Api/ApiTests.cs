using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QMSteel.Api.DTOs;

namespace QMSteel.Tests.Api
{
    // Pruebas de punta a punta por HTTP: autenticación, códigos de respuesta y formato de errores.
    public class ApiTests : IDisposable
    {
        private readonly ApiFactory _factory = new();

        public void Dispose() => _factory.Dispose();

        [Fact]
        public async Task WithoutToken_Returns401()
        {
            var response = await _factory.ClientFor(null).GetAsync("/api/soldadores");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UnregisteredIdentity_Returns403()
        {
            var response = await _factory.ClientFor("desconocido").GetAsync("/api/soldadores");

            await AssertErrorAsync(response, HttpStatusCode.Forbidden, "USER_NOT_REGISTERED");
        }

        [Fact]
        public async Task CreateWelder_Returns201WithLocationAndEnumsAsText()
        {
            var client = _factory.ClientFor();

            var created = await client.PostAsJsonAsync("/api/soldadores", new { employeeNumber = "E-1", fullName = "Soldador Uno", stampNumber = "W-07" });

            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var welder = await client.GetFromJsonAsync<JsonElement>(created.Headers.Location);
            Assert.Equal("W-07", welder.GetProperty("stampNumber").GetString());
            Assert.Equal("Welder", welder.GetProperty("type").GetString());
        }

        [Fact]
        public async Task DuplicateStamp_Returns409()
        {
            var client = _factory.ClientFor();
            await client.PostAsJsonAsync("/api/soldadores", new { employeeNumber = "E-1", fullName = "Soldador Uno", stampNumber = "W-07" });

            var response = await client.PostAsJsonAsync("/api/soldadores", new { employeeNumber = "E-2", fullName = "Soldador Dos", stampNumber = "W-07" });

            await AssertErrorAsync(response, HttpStatusCode.Conflict, "WELDER_STAMP_DUPLICATE");
        }

        [Fact]
        public async Task UnknownId_Returns404()
        {
            var response = await _factory.ClientFor().GetAsync($"/api/equipos/{Guid.NewGuid()}");

            await AssertErrorAsync(response, HttpStatusCode.NotFound, "EQUIPMENT_NOT_FOUND");
        }

        [Fact]
        public async Task BusinessRuleViolation_Returns422WithCode()
        {
            var response = await _factory.ClientFor().PostAsJsonAsync("/api/equipos",
                new { code = "UT-01", name = "Ultrasonido", category = "NdtEquipment", calibrationIntervalMonths = 0 });

            await AssertErrorAsync(response, HttpStatusCode.UnprocessableEntity, "EQUIPMENT_INTERVAL_INVALID");
        }

        [Fact]
        public async Task InvalidEnum_Returns400()
        {
            var response = await _factory.ClientFor().PostAsJsonAsync("/api/equipos",
                new { code = "UT-01", name = "Ultrasonido", category = "Desconocida", calibrationIntervalMonths = 12 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Auditor_CannotWrite()
        {
            await CreateUserAsync("auditor-test", "auditor@planta.test", "Auditor");

            var auditor = _factory.ClientFor("auditor-test");
            var read = await auditor.GetAsync("/api/soldadores");
            var write = await auditor.PostAsJsonAsync("/api/soldadores", new { employeeNumber = "E-1", fullName = "Soldador Uno", stampNumber = "W-07" });

            Assert.Equal(HttpStatusCode.OK, read.StatusCode);
            await AssertErrorAsync(write, HttpStatusCode.Forbidden, "USER_READ_ONLY");
        }

        [Fact]
        public async Task OnlyAdministrator_ManagesUsers()
        {
            await CreateUserAsync("inspector-test", "inspector@planta.test", "QcInspector");

            var response = await _factory.ClientFor("inspector-test").PostAsJsonAsync("/api/usuarios",
                new { identityId = "otro", email = "otro@planta.test", fullName = "Otro", role = "AreaUser" });

            await AssertErrorAsync(response, HttpStatusCode.Forbidden, "ROLE_NOT_ALLOWED");
        }

        [Fact]
        public async Task UploadedFile_CanBeDownloaded()
        {
            var client = _factory.ClientFor();
            using var form = new MultipartFormDataContent { { new ByteArrayContent("%PDF-1.7 prueba"u8.ToArray()), "archivo", "certificado.pdf" } };

            var uploaded = await client.PostAsync("/api/archivos", form);
            var download = await client.GetAsync(uploaded.Headers.Location);

            Assert.Equal(HttpStatusCode.Created, uploaded.StatusCode);
            Assert.Equal("%PDF-1.7 prueba", await download.Content.ReadAsStringAsync());
            Assert.Equal("certificado.pdf", download.Content.Headers.ContentDisposition?.FileName);
        }

        [Fact]
        public async Task UploadRejectsUnsupportedType()
        {
            using var form = new MultipartFormDataContent { { new ByteArrayContent([1, 2, 3]), "archivo", "macro.exe" } };

            var response = await _factory.ClientFor().PostAsync("/api/archivos", form);

            await AssertErrorAsync(response, HttpStatusCode.BadRequest, "FILE_TYPE_INVALID");
        }

        [Fact]
        public async Task OpenApiDocument_ListsEndpoints()
        {
            var document = await _factory.CreateClient().GetFromJsonAsync<JsonElement>("/openapi/v1.json");

            Assert.True(document.GetProperty("paths").TryGetProperty("/api/soldadores", out _));
        }

        private async Task CreateUserAsync(string identity, string email, string role)
        {
            var response = await _factory.ClientFor().PostAsJsonAsync("/api/usuarios",
                new { identityId = identity, email, fullName = $"Usuario {role}", role });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private static async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode status, string code)
        {
            Assert.Equal(status, response.StatusCode);
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.Equal(code, error?.Code);
        }
    }
}

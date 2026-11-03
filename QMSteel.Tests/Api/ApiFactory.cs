using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QMSteel.Tests.Api
{
    // Levanta la API completa sobre un SQLite en memoria. La autenticación real se reemplaza por un esquema
    // de prueba que toma la identidad del encabezado X-Test-Identity (sin encabezado = sin token).
    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        public const string AdminIdentity = "admin-test";

        private readonly string _connectionString = $"Data Source=qmsteel-api-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        private readonly SqliteConnection _keepAlive;

        public ApiFactory()
        {
            _keepAlive = new SqliteConnection(_connectionString);
            _keepAlive.Open();
        }

        public string StorageRoot { get; } = Path.Combine(Path.GetTempPath(), "qmsteel-tests", Guid.NewGuid().ToString("N"));

        public HttpClient ClientFor(string? identity = AdminIdentity)
        {
            var client = CreateClient();
            if (identity is not null)
                client.DefaultRequestHeaders.Add(TestAuthHandler.Header, identity);
            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("GeneralConfigurations:DataBaseProvider", "Sqlite");
            builder.UseSetting("GeneralConfigurations:ConnectionString", _connectionString);
            builder.UseSetting("GeneralConfigurations:FileStorageRoot", StorageRoot);
            builder.UseSetting("Bootstrap:Enabled", "true");
            builder.UseSetting("Bootstrap:TenantName", "Planta de Pruebas");
            builder.UseSetting("Bootstrap:TenantSlug", "planta-pruebas");
            builder.UseSetting("Bootstrap:AdminIdentityId", AdminIdentity);
            builder.UseSetting("Bootstrap:AdminEmail", "admin@planta.test");
            builder.UseSetting("Bootstrap:AdminFullName", "Administrador de Pruebas");

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                });
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _keepAlive.Dispose();
            if (Directory.Exists(StorageRoot))
                Directory.Delete(StorageRoot, recursive: true);
        }

        private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
        {
            public const string Scheme = "Test";
            public const string Header = "X-Test-Identity";

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                if (!Request.Headers.TryGetValue(Header, out var identity))
                    return Task.FromResult(AuthenticateResult.NoResult());

                var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, identity.ToString())], Scheme));
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme)));
            }
        }
    }
}

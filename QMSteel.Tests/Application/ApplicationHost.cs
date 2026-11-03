using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using QMSteel.Application;
using QMSteel.Application.Abstractions;
using QMSteel.Domain.Entities.Users;
using QMSteel.Infrastructure;
using QMSteel.Infrastructure.Persistence;
using QMSteel.Tests.Domain;
using QMSteel.Tests.Infrastructure;

namespace QMSteel.Tests.Application
{
    // Arma la aplicación completa (casos de uso + infraestructura real) sobre un SQLite en memoria.
    // Cada llamada a Get abre un scope nuevo, como si fuera otra petición HTTP.
    internal sealed class ApplicationHost : IDisposable
    {
        private readonly SqliteConnection _keepAlive;
        private readonly ServiceProvider _provider;
        private readonly List<IServiceScope> _scopes = new();

        private ApplicationHost(UserRole role)
        {
            var connectionString = $"Data Source=qmsteel-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
            _keepAlive = new SqliteConnection(connectionString);
            _keepAlive.Open();

            CurrentUser = new FakeCurrentUser { TenantId = TestData.TenantId };
            _provider = new ServiceCollection()
                .AddSingleton<ICurrentUserContext>(CurrentUser)
                .AddSingleton<TimeProvider>(new FixedTimeProvider())
                .AddInfrastructure(DatabaseProvider.Sqlite, connectionString)
                .AddApplication()
                .BuildServiceProvider();

            _provider.InitializeDatabaseAsync().GetAwaiter().GetResult();

            User = new User(TestData.TenantId, "idp|calidad", "calidad@planta.test", "Gerente de Calidad", role);
            CurrentUser.UserId = User.Id;
            Seed(User);
        }

        public static ApplicationHost Create(UserRole role = UserRole.QualityManager) => new(role);

        public FakeCurrentUser CurrentUser { get; }

        public User User { get; private set; }

        // Cambia el usuario en sesión, por ejemplo para que uno elabore y otro apruebe.
        public User SignInAs(UserRole role, string fullName)
        {
            User = new User(TestData.TenantId, $"idp|{Guid.NewGuid():N}", $"{role}@planta.test".ToLowerInvariant(), fullName, role);
            CurrentUser.UserId = User.Id;
            Seed(User);
            return User;
        }

        public T Get<T>() where T : notnull
        {
            var scope = _provider.CreateScope();
            _scopes.Add(scope);
            return scope.ServiceProvider.GetRequiredService<T>();
        }

        // Guarda datos de prueba directo en la base, sin pasar por los casos de uso.
        public void Seed(params object[] entities)
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<QMSteelDbContext>();
            context.AddRange(entities);
            context.SaveChanges();
        }

        public void Dispose()
        {
            _scopes.ForEach(s => s.Dispose());
            _provider.Dispose();
            _keepAlive.Dispose();
        }

        // Reloj fijo en la fecha de TestData; la zona local es UTC para que "hoy" no dependa de la máquina.
        private sealed class FixedTimeProvider : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => new(TestData.Now);

            public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
        }
    }
}

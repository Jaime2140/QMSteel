using QMSteel.Domain.Welding;
using QMSteel.Infrastructure.Persistence;
using QMSteel.Infrastructure.Persistence.Repositories;
using QMSteel.Tests.Domain;

namespace QMSteel.Tests.Infrastructure
{
    public class SqlitePersistenceTests : IDisposable
    {
        private readonly SqliteDatabase _database = new();
        private readonly FakeCurrentUser _user = new() { TenantId = TestData.TenantId };

        public void Dispose() => _database.Dispose();

        [Fact]
        public async Task Welder_RoundTripsWithQualifications()
        {
            await SaveAsync(TestData.QualifiedWelder());

            using var context = _database.CreateContext(_user);
            var welder = await new WelderRepository(context).GetByStampNumberAsync("W-07");

            Assert.NotNull(welder);
            var qualification = Assert.Single(welder.Qualifications);
            Assert.Equal(new[] { "1G", "2G", "3G" }, qualification.QualifiedPositions);
            Assert.Equal(TestData.Today.AddDays(-10), qualification.LastUsedOn);
        }

        [Fact]
        public async Task Wps_RoundTripsEssentialVariablesAndSignature()
        {
            await SaveAsync(TestData.ApprovedWps(WeldingProcess.GMAW));

            using var context = _database.CreateContext(_user);
            var wps = Assert.Single(await new WpsRepository(context).GetAllAsync(WeldingProcess.GMAW));

            Assert.Equal("C25", wps.Variables.ShieldingGas);
            Assert.Equal(110, wps.Variables.Amperage.Min);
            Assert.Equal("Aprobó", wps.ApprovedBy?.Meaning);
        }

        [Fact]
        public async Task Queries_OnlySeeCurrentTenant()
        {
            await SaveAsync(TestData.QualifiedWelder());

            using var context = _database.CreateContext(new FakeCurrentUser());

            Assert.Empty(await new WelderRepository(context).GetAllAsync());
        }

        [Fact]
        public async Task Equipment_CalibrationDueQueryRuns()
        {
            await SaveAsync(TestData.ReadyEquipment());

            using var context = _database.CreateContext(_user);
            var repository = new EquipmentRepository(context);

            Assert.Empty(await repository.GetCalibrationDueBeforeAsync(TestData.Today));
            Assert.Single(await repository.GetCalibrationDueBeforeAsync(TestData.Today.AddYears(1)));
        }

        [Fact]
        public async Task Folio_IsConsecutivePerTenant()
        {
            using var context = _database.CreateContext(_user);
            var generator = new FolioGenerator(context, _user);
            var otherUser = new FakeCurrentUser();
            using var otherContext = _database.CreateContext(otherUser);

            Assert.Equal("NC-2026-0001", await generator.NextAsync("nc", 2026));
            Assert.Equal("NC-2026-0002", await generator.NextAsync("NC", 2026));
            Assert.Equal("NC-2027-0001", await generator.NextAsync("NC", 2027));
            Assert.Equal("NC-2026-0001", await new FolioGenerator(otherContext, otherUser).NextAsync("NC", 2026));
        }

        [Fact]
        public async Task Update_AddingChildPersists()
        {
            var welder = TestData.QualifiedWelder();
            await SaveAsync(welder);

            using (var context = _database.CreateContext(_user))
            {
                var repository = new WelderRepository(context);
                var loaded = await repository.GetByIdAsync(welder.Id);
                loaded!.AddQualification(WeldingProcess.FCAW, "2F", Array.Empty<string>(), TestData.Today);
                repository.Update(loaded);
                await new UnitOfWork(context, new DomainEventDispatcher(EmptyServices.Instance)).SaveChangesAsync();
            }

            using var verify = _database.CreateContext(_user);
            var reloaded = await new WelderRepository(verify).GetByIdAsync(welder.Id);
            Assert.Equal(2, reloaded!.Qualifications.Count);
        }

        private async Task SaveAsync<T>(T aggregate) where T : class
        {
            using var context = _database.CreateContext(_user);
            context.Add(aggregate);
            await context.SaveChangesAsync();
        }

        private sealed class EmptyServices : IServiceProvider
        {
            public static readonly EmptyServices Instance = new();
            public object? GetService(Type serviceType) => null;
        }
    }
}

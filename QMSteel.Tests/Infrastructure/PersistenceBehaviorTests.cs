using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QMSteel.Application.Abstractions;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.Entities.Welders;
using QMSteel.Domain.Welding;
using QMSteel.Infrastructure.Persistence;
using QMSteel.Infrastructure.Persistence.Repositories;
using QMSteel.Tests.Domain;

namespace QMSteel.Tests.Infrastructure
{
    public class PersistenceBehaviorTests
    {
        [Fact]
        public void Audit_StampsCreationAndAssignsTenant()
        {
            var user = new FakeCurrentUser();
            using var context = TestDbContextFactory.Create(user);
            var welder = new Welder(Guid.Empty, "E-1", "Soldador", "W-1");
            context.Welders.Add(welder);

            context.ApplyAuditAndTenant();

            Assert.Equal(user.TenantId, welder.TenantId);
            Assert.Equal(user.UserId, welder.CreatedBy);
            Assert.NotEqual(default, welder.CreatedAtUtc);
        }

        [Fact]
        public void Audit_RejectsWritingIntoAnotherTenant()
        {
            using var context = TestDbContextFactory.Create(new FakeCurrentUser());
            context.Welders.Add(new Welder(Guid.NewGuid(), "E-1", "Soldador", "W-1"));

            Assert.Throws<InvalidOperationException>(context.ApplyAuditAndTenant);
        }

        [Fact]
        public void Audit_StampsUpdate()
        {
            var user = new FakeCurrentUser();
            using var context = TestDbContextFactory.Create(user);
            var welder = new Welder(user.TenantId!.Value, "E-1", "Soldador", "W-1");
            context.Welders.Attach(welder);

            welder.Deactivate();
            context.ChangeTracker.DetectChanges();
            context.ApplyAuditAndTenant();

            Assert.Equal(user.UserId, welder.UpdatedBy);
            Assert.NotNull(welder.UpdatedAtUtc);
        }

        [Fact]
        public void NewChildOfTrackedAggregate_IsAdded()
        {
            var user = new FakeCurrentUser();
            using var context = TestDbContextFactory.Create(user);
            var welder = new Welder(user.TenantId!.Value, "E-1", "Soldador", "W-1");
            context.Welders.Attach(welder);
            var repository = new WelderRepository(context);

            var qualification = welder.AddQualification(WeldingProcess.SMAW, "3G", Array.Empty<string>(), TestData.Today);
            repository.Update(welder);
            context.ChangeTracker.DetectChanges();

            Assert.Equal(EntityState.Added, context.Entry(qualification).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(welder).State);
        }

        [Fact]
        public void Folio_Format() => Assert.Equal("NC-2026-0007", FolioGenerator.Format("NC", 2026, 7));

        [Fact]
        public async Task Dispatcher_InvokesAllHandlersOfTheEvent()
        {
            var recorder = new Recorder();
            var provider = new ServiceCollection()
                .AddSingleton(recorder)
                .AddScoped<IDomainEventHandler<NonConformityOpened>, FirstHandler>()
                .AddScoped<IDomainEventHandler<NonConformityOpened>, SecondHandler>()
                .BuildServiceProvider();

            await new DomainEventDispatcher(provider).DispatchAsync(new NonConformityOpened(Guid.NewGuid(), "NC-1", Criticality.High, TestData.Now));

            Assert.Equal(new[] { "first", "second" }, recorder.Calls);
        }

        [Fact]
        public async Task UnitOfWork_DispatchesEventsBeforeSavingAndClearsThem()
        {
            var user = new FakeCurrentUser();
            var recorder = new Recorder();
            var provider = new ServiceCollection()
                .AddSingleton(recorder)
                .AddScoped<IDomainEventHandler<NonConformityOpened>, FirstHandler>()
                .BuildServiceProvider();
            using var context = new NoDatabaseContext(user);
            var nc = new NonConformity(user.TenantId!.Value, "NC-1", NonConformitySource.Internal, Criticality.Low,
                DefectType.Other, "Desviación", TestData.Today, user.UserId!.Value, TestData.Now);
            context.NonConformities.Add(nc);

            await new UnitOfWork(context, new DomainEventDispatcher(provider)).SaveChangesAsync();

            Assert.Equal(new[] { "first" }, recorder.Calls);
            Assert.Empty(nc.DomainEvents);
            Assert.True(context.Saved);
        }

        private sealed class Recorder
        {
            public List<string> Calls { get; } = new();
        }

        private sealed class FirstHandler(Recorder recorder) : IDomainEventHandler<NonConformityOpened>
        {
            public Task HandleAsync(NonConformityOpened domainEvent, CancellationToken ct = default)
            {
                recorder.Calls.Add("first");
                return Task.CompletedTask;
            }
        }

        private sealed class SecondHandler(Recorder recorder) : IDomainEventHandler<NonConformityOpened>
        {
            public Task HandleAsync(NonConformityOpened domainEvent, CancellationToken ct = default)
            {
                recorder.Calls.Add("second");
                return Task.CompletedTask;
            }
        }

        // Registra que se pidió guardar sin ir a la base.
        private sealed class NoDatabaseContext(ICurrentUserContext user) : QMSteelDbContext(
            new DbContextOptionsBuilder<QMSteelDbContext>().UseNpgsql("Host=localhost").Options, user, TimeProvider.System)
        {
            public bool Saved { get; private set; }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                Saved = true;
                return Task.FromResult(0);
            }
        }
    }
}

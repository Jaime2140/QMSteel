using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QMSteel.Application.Abstractions;
using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Entities.Documents;
using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Entities.Receiving;
using QMSteel.Domain.Entities.Suppliers;
using QMSteel.Domain.Entities.Tenants;
using QMSteel.Domain.Entities.Training;
using QMSteel.Domain.Entities.Users;
using QMSteel.Domain.Entities.Welders;

namespace QMSteel.Infrastructure.Persistence
{
    public class QMSteelDbContext : DbContext
    {
        private readonly ICurrentUserContext _currentUser;
        private readonly TimeProvider _timeProvider;

        public QMSteelDbContext(DbContextOptions<QMSteelDbContext> options, ICurrentUserContext currentUser, TimeProvider timeProvider)
            : base(options)
        {
            _currentUser = currentUser;
            _timeProvider = timeProvider;
        }

        // Se evalúa en cada consulta: el filtro global de tenant usa el valor de la petición en curso.
        public Guid? CurrentTenantId => _currentUser.TenantId;

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<User> Users => Set<User>();
        public DbSet<ControlledDocument> ControlledDocuments => Set<ControlledDocument>();
        public DbSet<Equipment> Equipments => Set<Equipment>();
        public DbSet<Welder> Welders => Set<Welder>();
        public DbSet<Wps> Wpss => Set<Wps>();
        public DbSet<Pqr> Pqrs => Set<Pqr>();
        public DbSet<Inspector> Inspectors => Set<Inspector>();
        public DbSet<TrainingPlan> TrainingPlans => Set<TrainingPlan>();
        public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<ReceivingInspection> ReceivingInspections => Set<ReceivingInspection>();
        public DbSet<NonConformity> NonConformities => Set<NonConformity>();
        public DbSet<Audit> Audits => Set<Audit>();
        public DbSet<ManagementReview> ManagementReviews => Set<ManagementReview>();
        public DbSet<Inspection> Inspections => Set<Inspection>();
        public DbSet<ComplianceRequirement> ComplianceRequirements => Set<ComplianceRequirement>();
        public DbSet<RequirementAssessment> RequirementAssessments => Set<RequirementAssessment>();
        public DbSet<QualityObjective> QualityObjectives => Set<QualityObjective>();
        public DbSet<FolioSequence> FolioSequences => Set<FolioSequence>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Enums como texto: la base queda legible y no se rompe si cambia el orden de los valores.
            configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(40);
            configurationBuilder.Properties<decimal>().HavePrecision(18, 4);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(QMSteelDbContext).Assembly);

            // Solo entidades del dominio: llamar Entity() sobre un objeto de valor lo volvería a registrar como entidad.
            var domainTypes = modelBuilder.Model.GetEntityTypes()
                .Where(t => !t.IsOwned() && typeof(Entity).IsAssignableFrom(t.ClrType))
                .Select(t => t.ClrType)
                .ToList();

            foreach (var clrType in domainTypes)
            {
                var builder = modelBuilder.Entity(clrType);

                // Los Id los genera el dominio; así EF trata como nuevas las entidades hijas que se agregan al agregado.
                builder.Property(nameof(Entity.Id)).ValueGeneratedNever();

                // Las entidades hijas no tienen DbSet: se les da nombre de tabla en plural como al resto.
                if (!typeof(AggregateRoot).IsAssignableFrom(clrType) && modelBuilder.Model.FindEntityType(clrType)!.GetTableName() == clrType.Name)
                    builder.ToTable(clrType.Name.EndsWith('y') ? clrType.Name[..^1] + "ies" : clrType.Name + "s");

                if (typeof(AggregateRoot).IsAssignableFrom(clrType))
                {
                    builder.Ignore(nameof(AggregateRoot.DomainEvents));

                    // xmin es columna de sistema de PostgreSQL; en SQLite (solo desarrollo) no hay concurrencia optimista.
                    if (Database.IsNpgsql())
                        builder.Property<uint>("xmin").IsRowVersion();
                }

                if (typeof(ITenantEntity).IsAssignableFrom(clrType))
                {
                    builder.HasIndex(nameof(ITenantEntity.TenantId));
                    builder.HasQueryFilter(TenantFilter(clrType));
                }
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditAndTenant();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyAuditAndTenant();
            return base.SaveChanges();
        }

        internal void ApplyAuditAndTenant()
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var userId = _currentUser.UserId;
            var tenantId = _currentUser.TenantId;

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = now;
                    entry.Entity.CreatedBy = userId;

                    if (entry.Entity is TenantEntity tenantEntity)
                    {
                        if (tenantEntity.TenantId == Guid.Empty && tenantId is not null)
                            tenantEntity.TenantId = tenantId.Value;
                        else if (tenantId is not null && tenantEntity.TenantId != tenantId)
                            throw new InvalidOperationException("No se pueden guardar datos de otro tenant.");
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAtUtc = now;
                    entry.Entity.UpdatedBy = userId;
                }
            }
        }

        private LambdaExpression TenantFilter(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var entityTenant = Expression.Convert(Expression.Property(parameter, nameof(ITenantEntity.TenantId)), typeof(Guid?));
            var currentTenant = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
            return Expression.Lambda(Expression.Equal(entityTenant, currentTenant), parameter);
        }
    }
}

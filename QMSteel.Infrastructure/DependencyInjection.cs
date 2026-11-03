using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QMSteel.Application.Abstractions;
using QMSteel.Domain.Repositories;
using QMSteel.Infrastructure.Files;
using QMSteel.Infrastructure.Persistence;
using QMSteel.Infrastructure.Persistence.Repositories;

namespace QMSteel.Infrastructure
{
    public static class DependencyInjection
    {
        // La API debe registrar su propia implementación de ICurrentUserContext (usuario y tenant del token).
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, DatabaseProvider provider, string connectionString,
            string fileStorageRoot = "storage")
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Falta la cadena de conexión 'QMSteel'.", nameof(connectionString));

            services.AddDbContext<QMSteelDbContext>(options => options.UseQMSteelDatabase(provider, connectionString));
            services.TryAddSingleton(TimeProvider.System);
            services.AddScoped<IFileStorage>(sp => new LocalFileStorage(fileStorageRoot, sp.GetRequiredService<ICurrentUserContext>()));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IFolioGenerator, FolioGenerator>();

            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IControlledDocumentRepository, ControlledDocumentRepository>();
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IWelderRepository, WelderRepository>();
            services.AddScoped<IWpsRepository, WpsRepository>();
            services.AddScoped<IPqrRepository, PqrRepository>();
            services.AddScoped<IInspectorRepository, InspectorRepository>();
            services.AddScoped<ITrainingPlanRepository, TrainingPlanRepository>();
            services.AddScoped<ITrainingSessionRepository, TrainingSessionRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IReceivingInspectionRepository, ReceivingInspectionRepository>();
            services.AddScoped<INonConformityRepository, NonConformityRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IManagementReviewRepository, ManagementReviewRepository>();
            services.AddScoped<IInspectionRepository, InspectionRepository>();
            services.AddScoped<IComplianceRequirementRepository, ComplianceRequirementRepository>();
            services.AddScoped<IRequirementAssessmentRepository, RequirementAssessmentRepository>();
            services.AddScoped<IQualityObjectiveRepository, QualityObjectiveRepository>();

            return services;
        }

        // SQLite crea el esquema directo desde el modelo: si el modelo cambia, se borra el archivo y se vuelve a crear.
        // En PostgreSQL se aplican las migraciones, que se generarán cuando el modelo esté estable.
        public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
        {
            await using var scope = services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<QMSteelDbContext>();

            if (context.Database.IsSqlite())
                await context.Database.EnsureCreatedAsync(ct);
            else
                await context.Database.MigrateAsync(ct);
        }
    }
}

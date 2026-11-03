using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QMSteel.Application.Abstractions;
using QMSteel.Application.Audits;
using QMSteel.Application.Bootstrap;
using QMSteel.Application.Common;
using QMSteel.Application.Compliance;
using QMSteel.Application.Dashboard;
using QMSteel.Application.Documents;
using QMSteel.Application.Equipments;
using QMSteel.Application.Inspections;
using QMSteel.Application.NonConformities;
using QMSteel.Application.Personnel;
using QMSteel.Application.Procedures;
using QMSteel.Application.Suppliers;
using QMSteel.Application.Training;
using QMSteel.Application.Users;
using QMSteel.Application.Welders;

namespace QMSteel.Application
{
    public static class DependencyInjection
    {
        // Requiere que la API registre ICurrentUserContext y que la infraestructura registre repositorios y UnitOfWork.
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.TryAddSingleton(TimeProvider.System);
            services.AddScoped<ISignatureService, SignatureService>();
            services.AddScoped<UseCaseContext>();
            services.AddScoped<NonConformityIntake>();

            services.AddScoped<DocumentService>();
            services.AddScoped<EquipmentService>();
            services.AddScoped<WelderService>();
            services.AddScoped<ProcedureService>();
            services.AddScoped<InspectorService>();
            services.AddScoped<TrainingService>();
            services.AddScoped<SupplierService>();
            services.AddScoped<ReceivingService>();
            services.AddScoped<NonConformityService>();
            services.AddScoped<AuditService>();
            services.AddScoped<ManagementReviewService>();
            services.AddScoped<InspectionService>();
            services.AddScoped<ComplianceService>();
            services.AddScoped<QualityObjectiveService>();
            services.AddScoped<DashboardService>();
            services.AddScoped<AlertService>();
            services.AddScoped<UserService>();
            services.AddScoped<TenantService>();
            services.AddScoped<TenantBootstrapper>();

            AddDomainEventHandlers(services);
            return services;
        }

        private static void AddDomainEventHandlers(IServiceCollection services)
        {
            var handlerTypes = typeof(DependencyInjection).Assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                    .Select(i => (Service: i, Implementation: t)));

            foreach (var (service, implementation) in handlerTypes)
                services.AddScoped(service, implementation);
        }
    }
}

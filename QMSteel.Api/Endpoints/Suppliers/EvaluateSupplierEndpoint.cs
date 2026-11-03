using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Suppliers
{
    public static class EvaluateSupplierEndpoint
    {
        private const string ENDPOINT_NAME = "Evaluar Proveedor";

        public static RouteGroupBuilder MapEvaluateSupplier(this RouteGroupBuilder app)
        {
            app.MapPost("proveedores/{id:guid}/evaluaciones", async (Guid id, [FromBody] EvaluateSupplierRequest request, SupplierService supplierService, CancellationToken ct) =>
            {
                var result = await supplierService.EvaluateAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<SupplierDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra la evaluación inicial o periódica; el score define su estatus en la AVL")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}

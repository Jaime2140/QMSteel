using QMSteel.Api.DTOs.Configurations;
using QMSteel.Api.Extensions;
using QMSteel.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var generalConfigurations = builder.Configuration.GetSection(GeneralConfigurations.SectionName).Get<GeneralConfigurations>()
    ?? throw new InvalidOperationException($"Falta la sección '{GeneralConfigurations.SectionName}' en la configuración.");

builder.Services.InyectarServicios(builder.Configuration);
builder.Services.InyectarContextos(generalConfigurations);
builder.Services.ConfigureAuthentication();
builder.Services.ConfigurarSwagger();

var app = builder.Build();

app.UseMiddleware<ErrorHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "QMSteel API"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CurrentUserMiddleware>();

app.MapEndpoints();
await app.InicializarBaseDeDatosAsync();

app.Run();

// Permite a las pruebas de integración levantar la API con WebApplicationFactory.
public partial class Program;

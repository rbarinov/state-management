using System.Text.Json.Serialization;
using Carter;
using StateManagement.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using StateManagement;

var builder = WebApplication.CreateBuilder(args);

var poolingConfiguration = builder.Configuration.GetSection("Pooling")
    .Get<PoolingConfigurationSection>();

builder.Services.AddDbContext<StateManagementDbContext>(
    (provider, optionsBuilder) =>
    {
        optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
            .UseSnakeCaseNamingConvention();
    },
    ServiceLifetime.Scoped
);

builder.Services.ConfigureHttpJsonOptions(
    e =>
    {
        e.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        e.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
);

builder.Services.Configure<KestrelServerOptions>(
    builder.Configuration.GetSection("Kestrel")
);

builder.Services.AddLogging(
    e => e
        .AddSimpleConsole(c => c.SingleLine = true)
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCarter();

builder.Services.AddMediatR(
    e => { e.RegisterServicesFromAssemblyContaining<Program>(); }
);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(
    e =>
    {
        e.DisplayOperationId();
        e.EnableTryItOutByDefault();
        e.DisplayRequestDuration();
    }
);

app.MapHealthChecks("/healthz");

app.MapCarter();

await app.RunAsync();

using System.Text.Json.Serialization;
using Carter;
using Events.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<EventDbContext>(
    (provider, optionsBuilder) =>
    {
        optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("db"))
            .UseSnakeCaseNamingConvention();
    }
);

builder.Services.ConfigureHttpJsonOptions(
    e =>
    {
        e.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        e.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
);

builder.Services.AddLogging(
    e => e
        .AddSimpleConsole(c => c.SingleLine = true)
    // .SetMinimumLevel(LogLevel.Warning)
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCarter();

builder.Services.AddMediatR(
    e => { e.RegisterServicesFromAssemblyContaining<Program>(); }
);

builder.Services.Configure<KestrelServerOptions>(
    options => { options.Limits.MaxRequestBodySize = 268_435_456; }
);

var app = builder.Build();

app.MapCarter();

app.UseSwagger();
app.UseSwaggerUI(e => e.DisplayOperationId());

await app.RunAsync();

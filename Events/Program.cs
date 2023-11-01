using System.Text.Json.Serialization;
using Carter;
using Events.Data;
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

var app = builder.Build();

app.MapCarter();

app.UseSwagger();
app.UseSwaggerUI(e => e.DisplayOperationId());

app.Run();

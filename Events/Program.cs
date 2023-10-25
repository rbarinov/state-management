using System.Text.Json.Serialization;
using Carter;
using Events.CompiledDataContext;
using Events.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<EventDbContext>(
    (provider, optionsBuilder) =>
    {
        optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("db"));
        optionsBuilder.UseModel(EventDbContextModel.Instance);
    }
);

builder.Services.ConfigureHttpJsonOptions(
    e =>
    {
        e.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        e.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
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
app.UseSwaggerUI();

app.Run();

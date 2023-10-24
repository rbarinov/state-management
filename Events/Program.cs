using System.Text.Json.Serialization;
using Events.CompiledDataContext;
using Events.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<EventDbContext>(
    (provider, optionsBuilder) =>
    {
        optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=postgres;Database=events");
        optionsBuilder.UseModel(EventDbContextModel.Instance);
    }
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(
    e => { e.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; }
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet(
        "/streams",
        async ([FromServices] EventDbContext db) =>
        {
            var streams = await db.Streams.ToListAsync();

            return streams;
        }
    )
    .WithOpenApi();

app.MapGet(
        "/streams/{streamId}/events",
        async ([FromServices] EventDbContext db, string streamId) =>
        {
            var events = await db.Events
                .Where(e => e.StreamId == streamId)
                .ToListAsync();

            return events;
        }
    )
    .WithOpenApi();

app.MapPost(
    "/streams/{streamId}/events",
    async ([FromServices] EventDbContext db, string streamId, [FromBody] EventModel model) =>
    {
        StreamDto? stream;

        if (model.ExpectedVersion == -1)
        {
            stream = new StreamDto
            {
                StreamId = streamId,
                Version = -1
            };

            db.Streams.Add(stream);
        }
        else
        {
            stream = await db.Streams.FirstOrDefaultAsync(e => e.StreamId == streamId);
        }

        if (stream == null || stream.Version != model.ExpectedVersion)
        {
            return Results.BadRequest();
        }

        stream.Version++;

        var ev = new EventDto
        {
            StreamId = streamId,
            Version = stream.Version,
            GlobalVersion = 0,
            Type = model.Type,
            Payload = Convert.FromBase64String(model.Payload64)
        };

        db.Events.Add(ev);

        await db.SaveChangesAsync();

        return Results.Created($"/streams/{streamId}/events/{stream.Version}", ev);
    }
);

app.MapGet(
        "/streams/{streamId}/events/{version}",
        async ([FromRoute] string streamId, [FromRoute] int version, [FromServices] EventDbContext db) =>
        {
            var ev = await db.Events
                .Where(e => e.StreamId == streamId && e.Version == version)
                .FirstOrDefaultAsync();

            if (ev == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ev);
        }
    )
    .WithOpenApi();

app.Run();

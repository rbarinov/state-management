using System.Text.Json.Serialization;
using Events.CompiledDataContext;
using Events.Data;
using Events.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<EventDbContext>(
    (provider, optionsBuilder) =>
    {
        optionsBuilder.UseNpgsql("Host=host.docker.internal;Username=postgres;Password=postgres;Database=events");
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
            var streams = await db.Streams
                .Select(
                    e => new StreamModelOut
                    {
                        StreamId = e.StreamId,
                        Version = e.Version
                    }
                )
                .ToListAsync();

            return streams;
        }
    )
    .WithOpenApi();

app.MapGet(
        "/streams/{streamId}/events",
        async (
            [FromServices] EventDbContext db,
            string streamId,
            int page = 1,
            int pageSize = 5,
            int? fromVersion = null
        ) =>
        {
            var q = db.Events
                .Where(e => e.StreamId == streamId);

            if (fromVersion.HasValue)
            {
                q = q.Where(e => e.Version > fromVersion.Value);
            }

            var events = await q
                .OrderBy(e => e.GlobalVersion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(
                    e => new EventModelOut
                    {
                        GlobalVersion = e.GlobalVersion,
                        StreamId = e.StreamId,
                        Version = e.Version,
                        Type = e.Type,
                        Payload64 = Convert.ToBase64String(e.Payload)
                    }
                )
                .ToListAsync();

            return events;
        }
    )
    .WithOpenApi();

app.MapPost(
    "/streams/{streamId}/events",
    async ([FromServices] EventDbContext db, string streamId, [FromBody] EventModelIn model) =>
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

        var response = new EventModelOut
        {
            GlobalVersion = ev.GlobalVersion,
            StreamId = streamId,
            Version = stream.Version,
            Type = ev.Type,
            Payload64 = Convert.ToBase64String(ev.Payload)
        };

        return Results.Created($"/streams/{streamId}/events/{stream.Version}", response);
    }
);

app.MapGet(
        "/streams/{streamId}/events/{version}",
        async ([FromRoute] string streamId, [FromRoute] int version, [FromServices] EventDbContext db) =>
        {
            var ev = await db.Events
                .Where(e => e.StreamId == streamId && e.Version == version)
                .Select(
                    e => new EventModelOut
                    {
                        GlobalVersion = e.GlobalVersion,
                        StreamId = e.StreamId,
                        Version = e.Version,
                        Type = e.Type,
                        Payload64 = Convert.ToBase64String(e.Payload)
                    }
                )
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

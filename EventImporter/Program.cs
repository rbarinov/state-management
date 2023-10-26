using EventImporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices(
    e =>
    {
        e.AddLogging(
            c => c
                .AddSimpleConsole(cc => cc.SingleLine = true)
                .SetMinimumLevel(LogLevel.Warning)
        );

        e.AddSingleton<EventStoreReader>();
        e.AddHttpClient();

        e.AddTransient<EventsClient>(
            c => new EventsClient(
                "http://localhost:5208/",
                c.GetRequiredService<HttpClient>()
            )
        );

        e.AddHostedService<Application>();
    }
);

var app = builder.Build();

await app.RunAsync();

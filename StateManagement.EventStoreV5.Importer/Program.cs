using EventImporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices(
    (context, e) =>
    {
        e.AddLogging(
            c => c
                .AddSimpleConsole(cc => cc.SingleLine = true)
        );

        e.AddSingleton<EventStoreReader>();
        e.AddHttpClient();

        e.AddSingleton<Configuration>(
            context.Configuration
                .GetSection("Configuration")
                .Get<Configuration>()!
        );

        e.AddTransient<EventsClient>(
            c => new EventsClient(
                c.GetRequiredService<Configuration>()
                    .StateManagementUrl,
                c.GetRequiredService<HttpClient>()
            )
        );

        e.AddHostedService<Application>();
    }
);

var app = builder.Build();

await app.RunAsync();

using EventImporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices(
    e =>
    {
        e.AddHostedService<Application>();

        e.AddHttpClient();

        e.AddTransient<EventClient>(
            c => new EventClient(
                "http://localhost:5208/",
                c.GetRequiredService<HttpClient>()
            )
        );
    }
);

var app = builder.Build();

await app.RunAsync();
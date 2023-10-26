using System.Runtime.CompilerServices;
using EventImporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices(
    e =>
    {
        e.AddHostedService<Application>();

        e.AddHttpClient();

        e.AddLogging(c => c.AddSimpleConsole(cc => cc.SingleLine = true));

        e.AddTransient<EventsClient>(
            c => new EventsClient(
                "http://localhost:5208/",
                c.GetRequiredService<HttpClient>()
            )
        );
    }
);

var app = builder.Build();

await app.RunAsync();

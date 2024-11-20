using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebSocketChat.Console;
using WebSocketChat.Shared.Endpoint;

Settings settings = new();

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"{nameof(Settings)}.json", optional: true, reloadOnChange: true)
    .Build();

configuration.Bind(settings);

settings.Url ??= "ws://localhost:5230/";
settings.PeerId ??= Guid.CreateVersion7();

string json = JsonSerializer.Serialize(settings);

File.WriteAllText($"{nameof(Settings)}.json", json);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddConfiguration(configuration);

builder.Services.AddLogging();

builder.Services.AddSingleton(x => settings);

builder.Services.AddSingleton(x => new ChatHubEndpoint(x.GetService<ILogger>())
{
    Host = settings.Url
});

builder.Services.AddHostedService<MainService>();

builder.Services.AddHostedService<SendTextService>();

IHost app = builder.Build();

await app.RunAsync();
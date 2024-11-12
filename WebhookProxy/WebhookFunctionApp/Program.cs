using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebhookFunctionApp.Services.BlobService;
using WebhookFunctionApp.Services.PayloadStore;
using WebhookFunctionApp.Services.RequestStore;
using WebhookFunctionApp.Services.RequestValidation;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;  // BLOG: Loading different settings based on environment

        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
    })
    .ConfigureLogging((context, loggingConfig) =>
    {
        var env = context.HostingEnvironment;

        if (env.IsDevelopment()) // BLOG: Varying the logging level
        {
            loggingConfig.SetMinimumLevel(LogLevel.Trace);
        }
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<IRequestValidator, RequestValidator>();
        services.AddSingleton<IPayloadStore, BlobPayloadStore>();
        services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();
    })
    .Build();

host.Run();
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebhookFunctionApp.Services.BlobService;
using WebhookFunctionApp.Services.PayloadStore;
using WebhookFunctionApp.Services.RequestStore;
using WebhookFunctionApp.Services.RequestValidation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    //.ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;  // BLOG: Loading different settings based on environment

        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
    })
    .ConfigureLogging((context, loggingConfig) =>
    {
        var env = context.HostingEnvironment;

        // https://learn.microsoft.com/en-us/azure/azure-functions/functions-app-settings#azure_functions_environment
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

        // ConfigureFunctionsWebApplication
        services.AddOptions<KestrelServerOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
                {
                    settings.AllowSynchronousIO = true;
                    configuration.Bind(settings);
                });
    })
    .Build();

host.Run();
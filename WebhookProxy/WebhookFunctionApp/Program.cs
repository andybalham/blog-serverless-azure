using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebhookFunctionApp.Services.RequestValidation;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IRequestValidator, RequestValidator>();
    })
    .Build();

host.Run();
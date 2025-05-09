using Microsoft.Extensions.DependencyInjection;
using Soenneker.Managers.Runners.Registrars;
using Soenneker.Loops.Runners.OpenApiClient.Utils;
using Soenneker.Loops.Runners.OpenApiClient.Utils.Abstract;
using Soenneker.Utils.File.Download.Registrars;

namespace Soenneker.Loops.Runners.OpenApiClient;

/// <summary>
/// Console type startup
/// </summary>
public static class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public static void ConfigureServices(IServiceCollection services)
    {
        services.SetupIoC();
    }

    public static IServiceCollection SetupIoC(this IServiceCollection services)
    {
        services.AddHostedService<ConsoleHostedService>();
        services.AddScoped<IFileOperationsUtil, FileOperationsUtil>();
        services.AddRunnersManagerAsScoped();
        services.AddFileDownloadUtilAsScoped();
        services.AddScoped<IOpenApiFixer, OpenApiFixer>();

        return services;
    }
}

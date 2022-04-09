using CoreLibrary.Helpers;
using CoreLibrary.SchedulerService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeerLibrary.Data;
using PeerLibrary.PeerApp;
using PeerLibrary.Settings;
using PeerLibrary.TokenProviders;
using PeerLibrary.UI;
using PeerLibrary.UI.Default;

namespace PeerLibrary.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceProvider ConfigureAppServices(this IServiceCollection services) => services.ConfigureAppServices<DefaultUI>();
    public static IServiceProvider ConfigureAppServices<TUI>(this IServiceCollection services) where TUI : class, IUI
    {
        PeerAppInfo appInfo = new Binspector().FindAppLibrary();
        return services.AddPeerLibrary<TUI>().AddAppLibrary(appInfo).BuildServiceProvider();
    }

    private static IServiceCollection AddPeerLibrary<TUI>(this IServiceCollection services) where TUI : class, IUI
    {
        return services
            .AddJsonConfiguration("peerlibrary.settings.json").Configure<HubSettings>("hub")
            .AddDbContext<PeerDbContext>(ServiceLifetime.Transient)
            .AddTransient<ISchedulerService, SchedulerService>()
            .AddSingleton<ITokenProvider, RopcTokenProvider>()
            .AddTransient<IUI, TUI>()
            .AddTransient<IHubClient, HubClient>();
    }

    private static IServiceCollection AddAppLibrary(this IServiceCollection services, PeerAppInfo appInfo)
    {
        var appConfig = (IPeerServiceConfiguration)appInfo.Required[typeof(IPeerServiceConfiguration)].CreateOrFail();
        appConfig.ConfigureServices(services);

        foreach (Type controllerType in appInfo.Controllers)
        {
            services.AddScoped(controllerType);
        }

        services.AddScoped(typeof(Dictionary<string, ControllerActionInfo>), _ => appInfo.RoutingMap);
        services.AddScoped<PeerRouting>();

        //Type concreteRouting = appInfo.Required[typeof(IPeerRouting)];
        //services.AddScoped(typeof(IPeerRouting), );
        //services.AddScoped<IPeerRouting>(sp => new PeerRouting(appInfo.RoutingMap));

        Type concreteStartup = appInfo.Required[typeof(IPeerStartup)];
        services.AddScoped(typeof(IPeerStartup), concreteStartup);

        return services;
    }

    public static JsonConfigurationBuilder AddJsonConfiguration(this IServiceCollection services, string fileName)
    {
        var dirInfo = Directory.GetParent(AppContext.BaseDirectory);
        if (dirInfo == null)
        {
            return new JsonConfigurationBuilder(services);
        }

        IConfigurationRoot? configRoot = new ConfigurationBuilder().SetBasePath(dirInfo.FullName).AddJsonFile(fileName, true).Build();
        return new JsonConfigurationBuilder(services, configRoot);
    }

    public static Task StartHubClient(this IServiceProvider serviceProvider)
    {
        IPeerStartup peerStartup = serviceProvider.GetRequiredService<IPeerStartup>();

        using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope())
        {
            scope.ServiceProvider.GetRequiredService<PeerDbContext>().Database.MigrateAsync();
            peerStartup.MigrateDatabase(scope);
        }

        return serviceProvider.GetRequiredService<IHubClient>().ExecuteDispose();
    }
}

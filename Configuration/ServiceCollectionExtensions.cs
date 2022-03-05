using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeerLibrary.Data;
using PeerLibrary.Settings;
using PeerLibrary.TokenProviders;
using PeerLibrary.UI;
using PeerLibrary.UI.Default;

namespace PeerLibrary.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPeerLibrary(this IServiceCollection services) => services.AddPeerLibrary<DefaultUI>();
        public static IServiceCollection AddPeerLibrary<TUI>(this IServiceCollection services) where TUI : class, IUI
        {
            return services
                .AddJsonConfiguration("peerlibrary.settings.json").Configure<HubSettings>("hub")
                .AddDbContext<PeerDbContext>()
                .AddSingleton<ITokenProvider, RopcTokenProvider>()
                .AddTransient<IUI, TUI>()
                .AddTransient<IHubClient, HubClient>();
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

        public static Task StartHubClient(this IServiceProvider serviceProvider, Action<AsyncServiceScope>? configScoped = null)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope())
            {
                scope.ServiceProvider.GetRequiredService<PeerDbContext>().Database.MigrateAsync();

                // TODO: for now, it's used so that LocalStation can migrate TestApp DB. In the future that should be done here, using reflection.
                configScoped?.Invoke(scope);
            }

            return serviceProvider.GetRequiredService<IHubClient>().ExecuteDispose();
        }
    }
}

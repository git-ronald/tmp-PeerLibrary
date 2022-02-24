using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeerLibrary.Settings;
using PeerLibrary.TokenProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerLibrary.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPeerLibrary(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AdddJsonConfiguration("peerlibrary.settings.json")
                .Configure<HubSettings>("hub")
                .AddSingleton<ITokenProvider, RopcTokenProvider>()
                .AddTransient<IHubClient, HubClient>();
        }

        public static JsonConfigurationBuilder AdddJsonConfiguration(this IServiceCollection serviceCollection, string fileName)
        {
            var dirInfo = Directory.GetParent(AppContext.BaseDirectory);
            if (dirInfo == null)
            {
                return new JsonConfigurationBuilder(serviceCollection);
            }

            IConfigurationRoot? configRoot = new ConfigurationBuilder().SetBasePath(dirInfo.FullName).AddJsonFile(fileName).Build();
            return new JsonConfigurationBuilder(serviceCollection, configRoot);
        }
    }
}

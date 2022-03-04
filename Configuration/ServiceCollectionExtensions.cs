﻿using Microsoft.EntityFrameworkCore;
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

        public static Task StartHubClient(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.GetServiceOrThrow<IServiceScopeFactory>().CreateAsyncScope())
            {
                scope.ServiceProvider.GetRequiredService<PeerDbContext>().Database.MigrateAsync();
            }

            return serviceProvider.GetServiceOrThrow<IHubClient>().ExecuteDispose();
        }

        public static TService GetServiceOrThrow<TService>(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetServiceOrThrow<TService>(() => new Exception($"Failed to create {typeof(TService).Name}"));
        }
        public static TService GetServiceOrThrow<TService>(this IServiceProvider serviceProvider, Func<Exception> getException)
        {
            TService? service = serviceProvider.GetService<TService>();
            if (service is null)
            {
                throw getException();
            }
            return service;
        }
    }
}

using PeerLibrary.Settings;
using PeerLibrary.TokenProviders;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace PeerLibrary
{
    internal class HubClient : IHubClient
    {
        private readonly HubSettings _settings;
        private readonly HubConnection _connection;

        public HubClient(IOptions<HubSettings> options, ITokenProvider tokenProvider)
        {
            _settings = options.Value;

            IHubConnectionBuilder connectionBuilder = new HubConnectionBuilder().WithUrl(_settings.HubUrl, options =>
            {
                options.AccessTokenProvider = tokenProvider.GetToken;
            });

            _connection = connectionBuilder.Build();

            _connection.On<string>("GetIt", message =>
            {
                Console.WriteLine($"Got message from hub: {message}");
            });

            _connection.On<string>("Test", message =>
            {
                Console.WriteLine($"Test event. Receiced message: {message}");
            });
        }

        public Task Start()
        {
            return _connection.StartAsync();
        }

        public Task Test()
        {
            return _connection.InvokeAsync("DoIt", "Little Pony");
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
    }
}

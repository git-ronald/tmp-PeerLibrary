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

            _connection.On<List<string>>("PeerRequest", async messages =>
            {
                messages.Add($"{DateTime.Now:HH:mm:ss} {Guid.NewGuid()} Peer");
                await _connection.InvokeAsync("PeerResponse", messages);

                Console.WriteLine();
                Console.WriteLine($"Hub called PeerRequest:");

                foreach (string msg in messages)
                {
                    Console.WriteLine(msg);
                }
            });
        }

        public Task Start()
        {
            return _connection.StartAsync();
        }

        public async Task Test()
        {
            string message = $"{DateTime.Now:HH:mm:ss} {Guid.NewGuid()} Peer";

            Console.WriteLine();
            Console.WriteLine($"Peer called TestRequest:");
            Console.WriteLine(message);

            await _connection.InvokeAsync("TestRequest", message);
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
    }
}

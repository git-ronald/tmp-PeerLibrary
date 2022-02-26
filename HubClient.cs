using PeerLibrary.Settings;
using PeerLibrary.TokenProviders;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PeerLibrary.UI;

namespace PeerLibrary
{
    internal class HubClient : IHubClient
    {
        private readonly HubSettings _settings;
        private readonly HubConnection _connection;
        private readonly IUI _ui;

        public HubClient(IOptions<HubSettings> options, ITokenProvider tokenProvider, IUI ui)
        {
            _settings = options.Value;
            _ui = ui;

            IHubConnectionBuilder connectionBuilder = new HubConnectionBuilder().WithUrl(_settings.HubUrl, options =>
            {
                options.AccessTokenProvider = tokenProvider.GetToken;
            });

            _connection = connectionBuilder.Build();

            // TODO NOW: what if hub server shuts down?

            _connection.On("TestResponse", () => {
                _ui.WriteLine($"{DateTime.Now} Reveived test response from {_settings.HubUrl}.");
            });

            _connection.On<List<string>>("PeerRequest", async messages =>
            {
                messages.Add($"{DateTime.Now:HH:mm:ss} {Guid.NewGuid()} Peer");
                await _connection.InvokeAsync("PeerResponse", messages);

                _ui.WriteLine();
                _ui.WriteLine($"Hub called PeerRequest:");

                foreach (string msg in messages)
                {
                    _ui.WriteLine(msg);
                }
            });
        }

        public async Task Start()
        {
            try
            {
                _ui.WriteLine("You can press Escape anytime to quit.");
                _ui.WriteLine();
                _ui.WriteLine("Starting Peer...");
                await _connection.StartAsync();

                _ui.WriteLine("Peer started.");

                _ui.WriteLine($"{DateTime.Now} Send test request...");

                await _connection.InvokeAsync("TestRequest");
            }
            catch (HttpRequestException)
            {
                _ui.WriteLine($"Failure connecting to hub.");
            }
            catch (Exception ex)
            {
                _ui.WriteLine($"Failed to start Peer: {ex.Message}");
            }

            _ui.WaitForExit();
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
    }
}

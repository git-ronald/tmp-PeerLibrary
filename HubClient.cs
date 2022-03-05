using CoreLibrary;
using CoreLibrary.PeerInterface;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PeerLibrary.ConstantValues;
using PeerLibrary.Data;
using PeerLibrary.Settings;
using PeerLibrary.TokenProviders;
using PeerLibrary.UI;
using System.Text;
using System.Text.Json;

namespace PeerLibrary
{
    internal class HubClient : ImmediatelyDisposable, IHubClient
    {
        private readonly HubSettings _hubSettings;
        private readonly PeerSettings _peerSettings;
        private readonly IUI _ui;
        private readonly HubConnection? _connection;
        private readonly PeerDbContext _peerDbContext;

        public HubClient(IOptions<HubSettings> hubOptions, IOptions<PeerSettings> peerOptions, IUI ui, ITokenProvider tokenProvider, PeerDbContext peerDbContext)
        {
            _hubSettings = hubOptions.Value;
            _peerSettings = peerOptions.Value;
            _ui = ui;
            _connection = BuildHubConnection(tokenProvider);
            _peerDbContext = peerDbContext;
        }

        private HubConnection? BuildHubConnection(ITokenProvider tokenProvider)
        {
            if (!_peerSettings.PeerId.HasValue)
            {
                return null;
            }

            IHubConnectionBuilder connectionBuilder = new HubConnectionBuilder()
                .WithUrl($"{_hubSettings.HubUrl}?clienttype=backend", options =>
                {
                    options.AccessTokenProvider = tokenProvider.GetToken;
                })
                .WithAutomaticReconnect();

            HubConnection connection = connectionBuilder.Build();
            AddConnectionEventHandlers(connection);

            return connection;
        }

        private void AddConnectionEventHandlers(HubConnection connection)
        {
            if (!_peerSettings.PeerId.HasValue)
            {
                return;
            }

            connection.Closed += OnonnectionClosed;

            connection.On("TestResponse", () =>
            {
                _ui.WriteTimeAndLine($"Reveived test response from {_hubSettings.HubUrl}.");
            });

            // TODO: protect incoming calls with extra "secret"
            connection.On("RequestPeerRegistrationInfo", RequestPeerRegistrationInfo);


            // TODO the rest is test code. Delete it at some time...

            connection.On<List<string>>("PeerRequest", async messages =>
            {
                messages.Add($"{DateTime.Now:HH:mm:ss} {Guid.NewGuid()} Peer");
                await Invoke("PeerResponse", messages);

                _ui.WriteLine();
                _ui.WriteTimeAndLine($"Hub called PeerRequest:");

                foreach (string msg in messages)
                {
                    _ui.WriteLine(msg);
                }
            });

            connection.On<List<string>>("HubResponse", messages =>
            {
                _ui.WriteLine();
                _ui.WriteTimeAndLine("Hub response");
                _ui.WriteLine($"Message count: {messages.Count}");
                _ui.WriteLine($"Last message: {messages.LastOrDefault()}");
            });
        }

        private async Task OnonnectionClosed(Exception? ex)
        {
            StringBuilder message = new($"{DateTime.Now} Hub connection closed");
            if (ex is not null)
            {
                message.Append(": ");
                message.Append(ex.Message);
            }
            message.Append('.');
            _ui.WriteLine(message);

            await ScheduleConnectAttempts();
        }

        private async Task RequestPeerRegistrationInfo()
        {
            if (!_peerSettings.PeerId.HasValue)
            {
                return;
            }

            string? settingValue = await _peerDbContext.AddSettingIfAbsent(SettingKeys.PeerNodeId, () => Guid.NewGuid());
            if (settingValue == null)
            {
                return;
            }

            _ui.WriteTimeAndLine($"Peer registration info requested.");

            await Invoke("PeerRegistrationInfoResponse", new PeerRegistrationInfo
            {
                PeerId = _peerSettings.PeerId.Value,
                PeerName = _peerSettings.PeerName,
                PeerNodeId = JsonSerializer.Deserialize<Guid>(settingValue)
            });
        }

        protected override async Task<IAsyncDisposable> Execute()
        {
            if (!_peerSettings.PeerId.HasValue)
            {
                ShowErrorConfigNotLoaded();
                return this;
            }

            _ui.WriteLine("Valid input:");
            _ui.WriteLine($"- {ConsoleKey.Escape} : quit");
            _ui.WriteLine($"- {ConsoleKey.Enter}  : retry connection");
            _ui.WriteLine();

            await SendTestRequest();
            await WaitForUserInput();

            return this;
        }

        private void ShowErrorConfigNotLoaded()
        {
            _ui.WriteLine("The peer settings configuration file could not be loaded.");
            _ui.WriteLine($"Press {ConsoleKey.Escape} to quit.");

            while (true)
            {
                ConsoleKeyInfo key = _ui.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    return;
                }
            }
        }

        private Task SendTestRequest()
        {
            _ui.WriteTimeAndLine("Send test request...");
            return Invoke("TestRequest");
        }

        private Task Invoke(string methodName) => TryInvoke(methodName, (c, n) => c.InvokeAsync(n));
        private Task Invoke<T>(string methodName, T arg) => TryInvoke(methodName, (c, n) => c.InvokeAsync<T>(n, arg));
        private async Task TryInvoke(string methodName, Func<HubConnection, string, Task> invoke)
        {
            try
            {
                if (_connection is null)
                {
                    return;
                }

                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await StartConnection();
                }
                if (_connection.State != HubConnectionState.Connected)
                {
                    _ui.WriteLine($"Failed to invoke {methodName} due to closed connection.");
                    return;
                }
                await invoke(_connection, methodName);
            }
            catch (HttpRequestException)
            {
                _ui.WriteLine($"Failure connecting to hub.");
                await ScheduleConnectAttempts();
            }
            catch (Exception ex)
            {
                _ui.WriteLine($"Failed starting connection: {ex.Message}");
                await ScheduleConnectAttempts();
            }
        }

        private async Task StartConnection()
        {
            if (_connection is null)
            {
                return;
            }

            _ui.WriteTimeAndLine("Starting hub connection...");
            await _connection.StartAsync();
            _ui.WriteTimeAndLine("Hub connection started.");
        }

        // TODO NOW: this should be called by TimeCompartmentScheduleService. Bug: right now the UI doesn't call ReadKey from IUI.
        private async Task ScheduleConnectAttempts()
        {
            if (_connection is null)
            {
                return;
            }

            await Task.Delay(_hubSettings.TryConnectInterval * 1000);

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await SendTestRequest();
            }
        }

        private async Task WaitForUserInput()
        {
            while (true)
            {
                ConsoleKeyInfo key = _ui.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
                if (key.Key == ConsoleKey.Enter && _connection?.State == HubConnectionState.Disconnected)
                {
                    await SendTestRequest();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}

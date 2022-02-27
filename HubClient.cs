using CoreLibrary;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PeerLibrary.Settings;
using PeerLibrary.TokenProviders;
using PeerLibrary.UI;
using System.Text;

namespace PeerLibrary
{
    internal class HubClient : ImmediatelyDisposable, IHubClient
    {
        private readonly HubSettings _settings;
        private readonly IUI _ui;
        private readonly HubConnection _connection;

        public HubClient(IOptions<HubSettings> options, IUI ui, ITokenProvider tokenProvider)
        {
            _settings = options.Value;
            _ui = ui;

            IHubConnectionBuilder connectionBuilder = new HubConnectionBuilder()
                .WithUrl($"{_settings.HubUrl}?clienttype=peer1", options =>
                {
                    options.AccessTokenProvider = tokenProvider.GetToken;
                })
                .WithAutomaticReconnect();

            _connection = connectionBuilder.Build();
            _connection.Closed += OnonnectionClosed;

            _connection.On("TestResponse", () =>
            {
                _ui.WriteTimeAndLine($"Reveived test response from {_settings.HubUrl}.");
            });

            _connection.On<List<string>>("PeerRequest", async messages =>
            {
                messages.Add($"{DateTime.Now:HH:mm:ss} {Guid.NewGuid()} Peer");
                await InvokeAsync("PeerResponse", messages);

                _ui.WriteLine();
                _ui.WriteTimeAndLine($"Hub called PeerRequest:");

                foreach (string msg in messages)
                {
                    _ui.WriteLine(msg);
                }
            });

            _connection.On<List<string>>("HubResponse", messages =>
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

        protected override async Task<IAsyncDisposable> Execute()
        {
            _ui.WriteLine("Valid input:");
            _ui.WriteLine($"- {ConsoleKey.Escape} : quit");
            _ui.WriteLine($"- {ConsoleKey.Enter}  : retry connection");
            _ui.WriteLine();

            await SendTestRequest();
            await WaitForUserInput();

            return this;
        }

        private Task SendTestRequest()
        {
            _ui.WriteTimeAndLine("Send test request...");
            return InvokeAsync("TestRequest");
        }

        private Task InvokeAsync(string methodName) => TryInvokeAsync(methodName, n => _connection.InvokeAsync(n));
        private Task InvokeAsync<T>(string methodName, T arg) => TryInvokeAsync(methodName, n => _connection.InvokeAsync<T>(n, arg));
        private async Task TryInvokeAsync(string methodName, Func<string, Task> invoke)
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await StartConnection();
                }
                if (_connection.State != HubConnectionState.Connected)
                {
                    _ui.WriteLine($"Failed to invoke {methodName} due to closed connection.");
                    return;
                }
                await invoke(methodName);
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
            _ui.WriteTimeAndLine("Starting hub connection...");
            await _connection.StartAsync();
            _ui.WriteTimeAndLine("Hub connection started.");
        }

        private async Task ScheduleConnectAttempts()
        {
            await Task.Delay(_settings.TryConnectInterval * 1000);

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
                if (key.Key == ConsoleKey.Enter && _connection.State == HubConnectionState.Disconnected)
                {
                    await SendTestRequest();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
    }
}

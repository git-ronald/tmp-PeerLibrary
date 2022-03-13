﻿using CoreLibrary;
using CoreLibrary.ConstantValues;
using CoreLibrary.Helpers;
using CoreLibrary.PeerInterface;
using CoreLibrary.SchedulerService;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PeerLibrary.ConstantValues;
using PeerLibrary.Data;
using PeerLibrary.Scheduler;
using PeerLibrary.Settings;
using PeerLibrary.TokenProviders;
using PeerLibrary.UI;
using System.Text;

namespace PeerLibrary
{
    internal class HubClient : ImmediatelyDisposable, IHubClient
    {
        private readonly HubSettings _hubSettings;
        private readonly PeerSettings _peerSettings;
        private readonly IUI _ui;
        private readonly HubConnection? _connection;
        private readonly PeerDbContext _peerDbContext;
        private readonly ISchedulerService _scheduler;
        private readonly ISchedulerConfig<TimeSpan> _fixedTimeSchedulerConfig;
        private readonly ISchedulerConfig<TimeCompartments> _compartmentSchedulerConfig;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly SchedulerState _schedulerState = new();
        private CancellationTokenSource _cancellation = new();

        public HubClient(IOptions<HubSettings> hubOptions, IOptions<PeerSettings> peerOptions, IUI ui, ITokenProvider tokenProvider, PeerDbContext peerDbContext, ISchedulerService scheduler, ISchedulerConfig<TimeSpan> fixedTimeSchedulerConfig, ISchedulerConfig<TimeCompartments> compartmentSchedulerConfig, IServiceScopeFactory scopeFactory)
        {
            _hubSettings = hubOptions.Value;
            _peerSettings = peerOptions.Value;
            _ui = ui;
            _connection = BuildHubConnection(tokenProvider);
            _peerDbContext = peerDbContext;
            _scheduler = scheduler;
            _fixedTimeSchedulerConfig = fixedTimeSchedulerConfig;
            _compartmentSchedulerConfig = compartmentSchedulerConfig;
            _scopeFactory = scopeFactory;
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

            connection.On(HubMessages.TestResponse, () => 
            {
                _ui.WriteTimeAndLine($"Reveived test response from {_hubSettings.HubUrl}.");
            });

            // TODO: protect incoming calls with extra "secret"
            connection.On<TimeSpan>(HubMessages.RequestPeerRegistrationInfo, RequestPeerRegistrationInfo);


            //// TODO the rest is test code. Delete it at some time...

            //connection.On<List<string>>("PeerRequest", async messages =>
            //{
            //    messages.Add($"{DateTime.Now:HH:mm:ss} {Guid.NewGuid()} Peer");
            //    await Invoke("PeerResponse", messages);

            //    _ui.WriteLine();
            //    _ui.WriteTimeAndLine($"Hub called PeerRequest:");

            //    foreach (string msg in messages)
            //    {
            //        _ui.WriteLine(msg);
            //    }
            //});

            //connection.On<List<string>>("HubResponse", messages =>
            //{
            //    _ui.WriteLine();
            //    _ui.WriteTimeAndLine("Hub response");
            //    _ui.WriteLine($"Message count: {messages.Count}");
            //    _ui.WriteLine($"Last message: {messages.LastOrDefault()}");
            //});
        }

        private Task OnonnectionClosed(Exception? ex)
        {
            StringBuilder message = new($"{DateTime.Now} Hub connection closed");
            if (ex is not null)
            {
                message.Append(": ");
                message.Append(ex.Message);
            }
            message.Append('.');
            _ui.WriteLine(message);

            _schedulerState.ConnectionPending = true;
            return Task.CompletedTask;
        }

        private async Task RequestPeerRegistrationInfo(TimeSpan suggestedSignOfLifeEvent)
        {
            try
            {
                if (!_peerSettings.PeerId.HasValue)
                {
                    return;
                }

                bool save = false;
                bool restartScheduler = false;

                var scope = _scopeFactory.CreateScope();
                using var peerDbContext = scope.ServiceProvider.GetRequiredService<PeerDbContext>();
                Guid peerNodeId = await peerDbContext.GetSetting<Guid>(SettingKeys.PeerNodeId);
                if (peerNodeId == Guid.Empty)
                {
                    peerNodeId = Guid.NewGuid();
                    await peerDbContext.SetSetting(SettingKeys.PeerNodeId, peerNodeId);
                    save = true;
                }

                TimeSpan signOfLifeEvent = await peerDbContext.GetSetting<TimeSpan>(SettingKeys.SignOfLifeEvent);
                if (signOfLifeEvent == TimeSpan.Zero)
                {
                    signOfLifeEvent = suggestedSignOfLifeEvent;
                    await peerDbContext.SetSetting(SettingKeys.SignOfLifeEvent, signOfLifeEvent);

                    save = true;
                    restartScheduler = true;
                }

                if (save)
                {
                    await peerDbContext.SaveChangesAsync();
                }
                if (restartScheduler)
                {
                    _cancellation.Cancel();
                    _cancellation = new CancellationTokenSource();
                    await StartScheduler();
                }

                _ui.WriteTimeAndLine($"Peer registration info requested.");

                await Invoke("PeerRegistrationInfoResponse", new PeerRegistrationInfo
                {
                    PeerId = _peerSettings.PeerId.Value,
                    PeerName = _peerSettings.PeerName,
                    PeerNodeId = peerNodeId,
                    ConfirmedSignOfLifeEvent = signOfLifeEvent
                });
            }
            catch (Exception ex)
            {
                _ui.WriteLine($"Failure in {nameof(RequestPeerRegistrationInfo)}: {ex.Message}");
            }
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
            await StartScheduler();
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

        private async Task StartScheduler()
        {
            var fixedTimeSchedule = await _fixedTimeSchedulerConfig.BuildSchedule(_schedulerState);

            TimeSpan signOfLifeEvent = await _peerDbContext.GetSetting<TimeSpan>(SettingKeys.SignOfLifeEvent);
            if (signOfLifeEvent != default)
            {
                foreach (int index in Enumerable.Range(0, 4))
                {
                    TimeSpan fixedTime = signOfLifeEvent.Add(TimeSpan.FromHours(index * 6));
                    fixedTimeSchedule.Ensure(fixedTime).Add(NotifySignOfLife);
                }
                //for (int index = 0; index < 4; index++)
                //{
                //    TimeSpan fixedTime = signOfLifeEvent.Add(TimeSpan.FromHours(index * 6));
                //    fixedTimeSchedule.Ensure(fixedTime).Add(NotifySignOfLife);
                //}
                //var secondEvent = signOfLifeEvent.Add(TimeSpan.FromHours(12));

                //fixedTimeSchedule.Ensure(signOfLifeEvent).Add(NotifySignOfLife);
                //fixedTimeSchedule.Ensure(secondEvent).Add(NotifySignOfLife);
            }

            var compartmentSchedule = await _compartmentSchedulerConfig.BuildSchedule(_schedulerState);
            compartmentSchedule.Ensure(TimeCompartments.EveryMinute).Add(
                _ =>
                {
                    if (_schedulerState.ConnectionPending)
                    {
                        return SendTestRequest();
                    }
                    return Task.CompletedTask;
                });

            var _ = _scheduler.Start(_cancellation.Token, fixedTimeSchedule, compartmentSchedule);
        }

        private Task NotifySignOfLife(CancellationToken cancellation)
        {
            _ui.WriteTimeAndLine("Notify sign of life.");
            return Invoke(HubMessages.NotifySignOfLife, cancellation);
        }

        private Task SendTestRequest()
        {
            _ui.WriteTimeAndLine("Send test request...");
            return Invoke(HubMessages.TestRequest);
        }

        private Task Invoke(string methodName, CancellationToken cancel = default) => TryInvoke(methodName, (c, n) => c.InvokeAsync(n, cancel));
        private Task Invoke<T>(string methodName, T arg, CancellationToken cancel = default) => TryInvoke(methodName, (c, n) => c.InvokeAsync<T>(n, arg, cancel));
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
                _schedulerState.ConnectionPending = false;
            }
            catch (HttpRequestException)
            {
                _ui.WriteLine($"Failure connecting to hub.");
                _schedulerState.ConnectionPending = true;
            }
            catch (Exception ex)
            {
                _ui.WriteLine($"Failed starting connection: {ex.Message}");
                _schedulerState.ConnectionPending = true;
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

        private async Task WaitForUserInput()
        {
            while (true)
            {
                ConsoleKeyInfo key = _ui.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
                if (key.Key == ConsoleKey.Enter)
                {
                    await SendTestRequest();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cancellation.Cancel();
            _cancellation.Dispose();

            if (_connection is not null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}

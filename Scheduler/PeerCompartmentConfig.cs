using CoreLibrary;
using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public class PeerCompartmentConfig : ISchedulerConfig<TimeCompartments>
    {
        public virtual Dictionary<TimeCompartments, SchedulerTaskList> BuildSchedule<TState>(TState? state = default(TState)) where TState : class
        {
            Dictionary<TimeCompartments, SchedulerTaskList> schedule = new();
            schedule[TimeCompartments.EveryMinute] = new()
            {
                stop => ScheduleEveryMinute(stop, state)
            };
            schedule[TimeCompartments.Every2Minutes] = new()
            {
                stop => ScheduleEvery2Minutes(stop, state)
            };

            return schedule;
        }

        private Task ScheduleEveryMinute(CancellationToken stoppingToken, object? state)
        {
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Every minute by PeerLibrary");
            return Task.CompletedTask;
        }

        private Task ScheduleEvery2Minutes(CancellationToken stoppingToken, object? state)
        {
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Every 2 minutes by PeerLibrary");
            return Task.CompletedTask;
        }
    }
}

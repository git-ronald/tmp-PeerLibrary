using CoreLibrary;
using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public class PeerCompartmentSchedulerConfig : ISchedulerConfig<object, TimeCompartments>
    {
        public PeerCompartmentSchedulerConfig()
        {
            Tasks[TimeCompartments.EveryMinute] = new()
            {
                ScheduleEveryMinute
            };

            Tasks[TimeCompartments.Every2Minutes] = new()
            {
                ScheduleEvery2Minutes
            };
        }

        public Dictionary<TimeCompartments, List<ScheduledTaskDelegate<object>>> Tasks { get; } = new();

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

using CoreLibrary;
using CoreLibrary.Helpers;
using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public class PeerCompartmentConfig : ISchedulerConfig<TimeCompartments>
    {
        public Dictionary<TimeCompartments, SchedulerTaskList> Schedule { get; } = new();

        public virtual Dictionary<TimeCompartments, SchedulerTaskList> BuildSchedule(SchedulerState state)
        {
            //Schedule.Ensure(TimeCompartments.EveryMinute).Add(
            //    cancel => ScheduleEveryMinute(cancel, state));

            //Schedule.Ensure(TimeCompartments.Every2Minutes).Add(
            //    cancel => ScheduleEvery2Minutes(cancel, state));

            return Schedule;
        }

        //private Task AttemptHubConnection(CancellationToken cancellation, SchedulerState state)
        //{
        //    return Task.CompletedTask;
        //}

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

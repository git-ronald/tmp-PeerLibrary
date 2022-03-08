using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public class PeerFixedTimeConfig : ISchedulerConfig<TimeSpan> //IFixedTimeConfig
    {
        public virtual Dictionary<TimeSpan, SchedulerTaskList> BuildSchedule<TState>(TState? state = default(TState)) where TState : class
        {
            DateTime date = DateTime.UtcNow.AddMinutes(1);
            TimeSpan nextEvent = new TimeSpan(date.Hour, date.Minute, 0);

            Dictionary<TimeSpan, SchedulerTaskList> schedule = new();
            schedule[nextEvent] = new()
            {
                token =>
                {
                    Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Fixed time by PeerLibrary {nextEvent}");
                    return Task.CompletedTask;
                }
            };

            return schedule;
        }
    }
}

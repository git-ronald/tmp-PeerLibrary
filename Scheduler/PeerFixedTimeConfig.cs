using CoreLibrary.Helpers;
using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public class PeerFixedTimeConfig : ISchedulerConfig<TimeSpan>
    {
        public Dictionary<TimeSpan, SchedulerTaskList> Schedule { get; } = new();

        public virtual Dictionary<TimeSpan, SchedulerTaskList> BuildSchedule(SchedulerState _)
        {
            //DateTime date = DateTime.UtcNow.AddMinutes(1);
            //TimeSpan nextEvent = new TimeSpan(date.Hour, date.Minute, 0);

            //Schedule.Ensure(nextEvent).Add(
            //    token =>
            //    {
            //        Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Fixed time by PeerLibrary {nextEvent}");
            //        return Task.CompletedTask;
            //    });

            return Schedule;
        }
    }
}

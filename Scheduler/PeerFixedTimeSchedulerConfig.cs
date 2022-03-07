using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public class PeerFixedTimeSchedulerConfig : ISchedulerConfig<SchedulerState, TimeSpan>
    {
        public PeerFixedTimeSchedulerConfig()
        {
            Tasks[new TimeSpan(21, 15, 0)] = new()
            {
                (token, state) =>
                {
                    Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Fixed time by PeerLibrary");
                    return Task.CompletedTask;
                }
            };
        }

        public Dictionary<TimeSpan, List<ScheduledTaskDelegate<SchedulerState>>> Tasks { get; } = new();
    }
}

using CoreLibrary;
using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public class PeerCompartmentConfig : ISchedulerConfig<TimeCompartments>
    {
        public Dictionary<TimeCompartments, SchedulerTaskList> Schedule { get; } = new();

        public virtual Task<Dictionary<TimeCompartments, SchedulerTaskList>> BuildSchedule(SchedulerState state)
        {
            return Task.FromResult(Schedule);
        }
    }
}

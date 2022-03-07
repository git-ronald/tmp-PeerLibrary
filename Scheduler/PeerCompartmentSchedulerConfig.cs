using CoreLibrary;
using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public class PeerCompartmentSchedulerConfig : ISchedulerConfig<SchedulerState, TimeCompartments>
    {
        public PeerCompartmentSchedulerConfig()
        {
            //Tasks[TimeCompartments.EveryMinute] = new()
            //{
            //    ScheduleEveryMinute
            //};

            //Tasks[TimeCompartments.Every2Minutes] = new()
            //{
            //    ScheduleEvery2Minutes
            //};
        }

        public Dictionary<TimeCompartments, List<ScheduledTaskDelegate<SchedulerState>>> Tasks { get; } = new();
    }
}

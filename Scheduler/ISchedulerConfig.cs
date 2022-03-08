using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public interface ISchedulerConfig<TKey> where TKey : notnull
    {
        Dictionary<TKey, SchedulerTaskList> BuildSchedule(SchedulerState state);
    }
}
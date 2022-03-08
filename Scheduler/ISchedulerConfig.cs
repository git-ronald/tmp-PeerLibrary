using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public interface ISchedulerConfig<TKey> where TKey : notnull
    {
        Dictionary<TKey, SchedulerTaskList> BuildSchedule<TState>(TState? state = default) where TState : class;
    }
}
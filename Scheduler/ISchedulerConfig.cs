using CoreLibrary.SchedulerService;

namespace PeerLibrary.Scheduler
{
    public interface ISchedulerConfig<TKey> where TKey : notnull
    {
        Task<Dictionary<TKey, SchedulerTaskList>> BuildSchedule(SchedulerState state);
    }
}
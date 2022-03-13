using CoreLibrary.Helpers;
using CoreLibrary.SchedulerService;
using Microsoft.EntityFrameworkCore;
using PeerLibrary.ConstantValues;
using PeerLibrary.Data;
using PeerLibrary.Data.Models;
using System.Text.Json;

namespace PeerLibrary.Scheduler
{
    public class PeerFixedTimeConfig : ISchedulerConfig<TimeSpan>
    {
        public Dictionary<TimeSpan, SchedulerTaskList> Schedule { get; } = new();

        public virtual Task<Dictionary<TimeSpan, SchedulerTaskList>> BuildSchedule(SchedulerState _)
        {
            return Task.FromResult(Schedule);
        }
    }
}

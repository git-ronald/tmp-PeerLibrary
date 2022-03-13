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
            //DateTime date = DateTime.UtcNow.AddMinutes(1);
            //TimeSpan nextEvent = new TimeSpan(date.Hour, date.Minute, 0);

            //Schedule.Ensure(nextEvent).Add(
            //    token =>
            //    {
            //        Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Fixed time by PeerLibrary {nextEvent}");
            //        return Task.CompletedTask;
            //    });

            return Task.FromResult(Schedule);
        }
    }
}

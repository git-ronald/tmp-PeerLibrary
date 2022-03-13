using Microsoft.EntityFrameworkCore;
using PeerLibrary.Data.Models;
using System.Reflection;
using System.Text.Json;

namespace PeerLibrary.Data
{
    public class PeerDbContext : DbContext
    {
        // TODO: in the future make GenericRepository and make PeerDbContext internal
        
        public DbSet<Setting> Settings => Set<Setting>();

        //public async Task<bool> UpdateSetting(object key, object value) => (await AddSettingIfAbsent(key, () => value, false)).Updated;

        //public async Task<string?> AddSettingIfAbsent(object key, Func<object> getValue) => (await AddSettingIfAbsent(key, getValue, true)).Value;
        //private async Task<(string? Value, bool Updated)> AddSettingIfAbsent(object key, Func<object> getValue, bool setNewValueOnly)
        //{
        //    string? stringKey = key.ToString();
        //    if (stringKey is null)
        //    {
        //        return (null, false);
        //    }

        //    var setting = await Settings.FirstOrDefaultAsync(s => s.Key == stringKey);
        //    if (setting is null)
        //    {
        //        string stringValue = JsonSerializer.Serialize(getValue());
        //        stringValue ??= String.Empty;

        //        await Settings.AddAsync(new Setting { Key = stringKey, Value = stringValue });
        //        await SaveChangesAsync();
        //        return (stringValue, true);
        //    }
        //    else
        //    {
        //        if (setNewValueOnly)
        //        {
        //            return (setting.Value, false);
        //        }

        //        string stringValue = JsonSerializer.Serialize(getValue());
        //        stringValue ??= String.Empty;
        //        bool updated = (stringValue != setting.Value);

        //        setting.Value = stringValue;
        //        await SaveChangesAsync();
        //        return (setting.Value, updated);
        //    }
        //}

        public async Task<T?> GetSetting<T>(object key)
        {
            string? stringKey = key.ToString();
            if (stringKey is null)
            {
                return default;
            }

            Setting? setting = await Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == stringKey);
            if (setting is null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(setting.Value);
        }

        public async Task SetSetting(object key, object value)
        {
            string? stringKey = key.ToString();
            if (stringKey is null)
            {
                return;
            }

            var setting = await Settings.FirstOrDefaultAsync(s => s.Key == stringKey);
            if (setting is null)
            {
                setting = new Setting { Key = stringKey };
                await AddAsync(setting);
            }

            setting.Value = JsonSerializer.Serialize(value);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "peer.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }
}

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

        public Task<string?> UpdateSetting(object key, object value) => AddSettingIfAbsent(key, () => value, false);

        public Task<string?> AddSettingIfAbsent(object key, Func<object> getValue) => AddSettingIfAbsent(key, getValue, true);
        private async Task<string?> AddSettingIfAbsent(object key, Func<object> getValue, bool setNewValueOnly)
        {
            string? stringKey = key.ToString();
            if (stringKey is null)
            {
                return null;
            }

            var setting = await Settings.FirstOrDefaultAsync(s => s.Key == stringKey);
            if (setting is null)
            {
                string stringValue = JsonSerializer.Serialize(getValue());
                stringValue ??= String.Empty;

                await Settings.AddAsync(new Setting { Key = stringKey, Value = stringValue });
                await SaveChangesAsync();
                return stringValue;
            }
            else
            {
                if (setNewValueOnly)
                {
                    return setting.Value;
                }

                string stringValue = JsonSerializer.Serialize(getValue());
                stringValue ??= String.Empty;

                setting.Value = stringValue;
                await SaveChangesAsync();
                return setting.Value;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "peer.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }
}

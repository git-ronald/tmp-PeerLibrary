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

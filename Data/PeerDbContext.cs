using Microsoft.EntityFrameworkCore;
using PeerLibrary.Data.Models;
using System.Reflection;
using System.Text.Json;

namespace PeerLibrary.Data
{
    public class PeerDbContext : DbContext
    {
        public DbSet<Setting> Settings => Set<Setting>();

        // TODO: in the future make GenericRepository and make PeerDbContext internal
        public async Task<string?> AddSettingIfAbsent(object key, Func<object> getValue)
        {
            string? stringKey = key.ToString();
            if (stringKey is null)
            {
                return null;
            }

            var setting = await Settings.FirstOrDefaultAsync(s => s.Key == stringKey);
            if (setting is not null)
            {
                return setting.Value;
            }

            string stringValue = JsonSerializer.Serialize(getValue());
            if (String.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            await Settings.AddAsync(new Setting { Key = stringKey, Value = stringValue });
            await SaveChangesAsync();

            return stringValue;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "peer.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }
}

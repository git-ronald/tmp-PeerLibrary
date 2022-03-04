using Microsoft.EntityFrameworkCore;
using PeerLibrary.Data.Models;
using System.Reflection;

namespace PeerLibrary.Data
{
    public class PeerDbContext : DbContext
    {
        public DbSet<Setting> Settings => Set<Setting>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "peer.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace PeerLibrary.PeerApp.Interfaces;

public interface IPeerStartup
{
    Task MigrateDatabase(AsyncServiceScope scope);
}

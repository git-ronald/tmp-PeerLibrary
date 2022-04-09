using Microsoft.Extensions.DependencyInjection;

namespace PeerLibrary.PeerApp;

public interface IPeerStartup
{
    Task MigrateDatabase(AsyncServiceScope scope);
}

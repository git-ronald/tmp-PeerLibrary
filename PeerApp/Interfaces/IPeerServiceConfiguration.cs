using Microsoft.Extensions.DependencyInjection;

namespace PeerLibrary.PeerApp.Interfaces;

public interface IPeerServiceConfiguration
{
    IServiceCollection ConfigureServices(IServiceCollection services);
}

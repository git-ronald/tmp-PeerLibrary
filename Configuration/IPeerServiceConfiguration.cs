using Microsoft.Extensions.DependencyInjection;

namespace PeerLibrary.Configuration;

public interface IPeerServiceConfiguration
{
    IServiceCollection ConfigureServices(IServiceCollection services);
}

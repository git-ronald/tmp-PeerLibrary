using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PeerLibrary.Configuration
{
    public class JsonConfigurationBuilder
    {
        private readonly IServiceCollection _services;
        private readonly IConfigurationRoot? _configRoot;

        public JsonConfigurationBuilder(IServiceCollection services, IConfigurationRoot? configRoot = null)
        {
            _services = services;
            _configRoot = configRoot;
        }

        public IServiceCollection Configure<T>(string section) where T : class
        {
            if (_configRoot == null)
            {
                return _services;
            }
            return _services.Configure<T>(_configRoot.GetSection(section));
        }
    }
}

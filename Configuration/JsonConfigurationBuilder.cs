using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerLibrary.Configuration
{
    public class JsonConfigurationBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfigurationRoot? _configRoot;

        public JsonConfigurationBuilder(IServiceCollection serviceCollection, IConfigurationRoot ? configRoot = null)
        {
            _serviceCollection = serviceCollection;
            _configRoot = configRoot;
        }

        public IServiceCollection Configure<T>(string section) where T : class
        {
            if (_configRoot == null)
            {
                return _serviceCollection;
            }
            return _serviceCollection.Configure<T>(_configRoot.GetSection(section));
        }
    }
}

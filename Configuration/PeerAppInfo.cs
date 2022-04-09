using System.Reflection;

namespace PeerLibrary.Configuration
{
    internal class PeerAppInfo
    {
        public Dictionary<Type, Type> Required { get; set; } = new();
        public List<Type> Controllers { get; set; } = new();
        public Dictionary<string, (Type Controller, MethodInfo Action)> RoutingMap { get; set; } = new();
    }
}

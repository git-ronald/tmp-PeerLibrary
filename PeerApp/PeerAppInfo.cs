using System.Reflection;

namespace PeerLibrary.PeerApp;

internal class PeerAppInfo
{
    public Dictionary<Type, Type> Required { get; set; } = new();
    public List<Type> Controllers { get; set; } = new();
    public Dictionary<string, ControllerActionInfo> RoutingMap { get; set; } = new();
}

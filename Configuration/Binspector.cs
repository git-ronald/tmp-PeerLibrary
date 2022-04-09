using CoreLibrary.Interfaces;
using System.Reflection;

namespace PeerLibrary.Configuration;

/// <summary>
/// It's the bin folder inspector!... Bin-spector! Get it? Haha.
/// </summary>
internal class Binspector
{
    private const string ControllerPrefix = "Controller";

    private readonly Type[] _requiredTypes = new[] { typeof(IPeerServiceConfiguration), typeof(IPeerRouting), typeof(IPeerStartup) };

    public PeerAppInfo FindAppLibrary()
    {
        DirectoryInfo? dirInfo = Directory.GetParent(AppContext.BaseDirectory);
        if (dirInfo == null)
        {
            throw new DirectoryNotFoundException("Unable to find bin directory."); // Should never happen
        }

        foreach (string libraryPath in Directory.GetFileSystemEntries(dirInfo.FullName, "*.dll"))
        {
            // TODO NOW: delete
            var fileName = Path.GetFileName(libraryPath);
            if (fileName == "TestAppLibrary.dll")
            {

            }

            var appTypes = GetAppTypes(libraryPath);
            if (!TryGetAppInfo(appTypes, out PeerAppInfo appInfo))
            {
                continue;
            }

            return appInfo;
        }

        throw new FileNotFoundException("Unable to find app library.");
    }

    private Type[] GetAppTypes(string libraryPath)
    {
        Assembly? assembly = Assembly.LoadFile(libraryPath);
        if (assembly == null)
        {
            throw new IOException($"Failed to load {libraryPath}");
        }

        return assembly.GetExportedTypes();
    }

    private bool TryGetAppInfo(Type[] libraryTypes, out PeerAppInfo appInfo)
    {
        PeerAppInfo result = new();

        foreach (Type type in libraryTypes)
        {
            Type? abstractType = _requiredTypes.FirstOrDefault(t => t.IsAssignableFrom(type));
            if (abstractType != null)
            {
                result.Required[abstractType] = type;
                continue;
            }

            if (type.Namespace?.StartsWith(ControllerPrefix) ?? false)
            {
                var controllerRouting = BuildRoutingMap(type);
                if (controllerRouting.Count == 0)
                {
                    continue;
                }

                result.Controllers.Add(type);
                result.RoutingMap = result.RoutingMap.Concat(controllerRouting).ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        if (result.Required.Count != _requiredTypes.Length)
        {
            appInfo = new PeerAppInfo();
            return false;
        }

        appInfo = result;
        return true;
    }

    private Dictionary<string, (Type Controller, MethodInfo Action)> BuildRoutingMap(Type controllerType)
    {
        var actions = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        if (actions.Length == 0)
        {
            return new Dictionary<string, (Type Controller, MethodInfo Action)>();
        }

        string path = (controllerType.Namespace ?? String.Empty).Substring(ControllerPrefix.Length).Replace('.', '/');
        return actions.ToDictionary(action => $"{path}/{action.Name}".ToLower(), action => (controllerType, action));
    }
}

using PeerLibrary.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PeerLibrary.PeerApp;

/// <summary>
/// It's the bin folder inspector!... Bin-spector! Get it? Haha.
/// </summary>
internal class Binspector
{
    private readonly Regex _controllerNamespaceRegex = new(@"^[^\.]+\.Controllers(\..+)$", RegexOptions.Compiled);

    private readonly Type[] _requiredTypes = new[] { typeof(IPeerServiceConfiguration), typeof(IPeerStartup) };

    public PeerAppInfo FindAppLibrary()
    {
        DirectoryInfo? dirInfo = Directory.GetParent(AppContext.BaseDirectory);
        if (dirInfo == null)
        {
            throw new DirectoryNotFoundException("Unable to find bin directory."); // Should never happen
        }

        foreach (string libraryPath in Directory.GetFileSystemEntries(dirInfo.FullName, "*.dll"))
        {
            var appTypes = GetAppTypes(libraryPath);
            if (TryGetAppInfo(appTypes, out PeerAppInfo appInfo))
            {
                return appInfo;
            }
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

            if (type.Namespace == null)
            {
                continue;
            }
            var namespaceMatch = _controllerNamespaceRegex.Match(type.Namespace);
            if (!namespaceMatch.Success)
            {
                continue;
            }

            var controllerRouting = MapControllerRouting(type, namespaceMatch.Groups[1].Value);
            if (controllerRouting.Count == 0)
            {
                continue;
            }

            result.Controllers.Add(type);
            result.RoutingMap = result.RoutingMap.Concat(controllerRouting).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        if (result.Required.Count != _requiredTypes.Length)
        {
            appInfo = new PeerAppInfo();
            return false;
        }

        appInfo = result;
        return true;
    }

    private Dictionary<string, ControllerActionInfo> MapControllerRouting(Type controllerType, string namespaceMatch)
    {
        var actions = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(m => GetControllerActionDefinition(m))
            .Where(d => d != null)
            .ToArray();

        if (actions.Length == 0)
        {
            return new Dictionary<string, ControllerActionInfo>();
        }

        string controllerPath = namespaceMatch.Replace('.', '/');

        return actions.Where(action => action != null).ToDictionary(
            action => $"{controllerPath}/{controllerType.Name}/{action?.MethodInfo.Name}".ToLower(),
            action => action ?? new());
    }

    private ControllerActionInfo? GetControllerActionDefinition(MethodInfo method)
    {
        if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return new(method, method.GetParameters().FirstOrDefault()?.ParameterType, method.ReturnType);
        }
        if (method.ReturnType == typeof(Task))
        {
            return new(method, method.GetParameters().FirstOrDefault()?.ParameterType, null);
        }
        return null;
    }
}

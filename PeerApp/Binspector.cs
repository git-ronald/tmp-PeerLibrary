using CoreLibrary.Helpers;
using PeerLibrary.PeerApp.Interfaces;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PeerLibrary.PeerApp;

/// <summary>
/// It's the bin folder inspector!... Bin-spector! Get it? Haha.
/// </summary>
internal class Binspector
{
    private readonly Regex _controllerInterfaceNsRegex = new(@"^[^\.]+\.PeerApp\.Interfaces\.Controllers\.?(.*)$", RegexOptions.Compiled);
    private readonly Regex _peerControllerNsRegex = new(@"^[^\.]+\.Controllers\.Peer\.?(.*)$", RegexOptions.Compiled);
    private readonly Regex _appControllerNsRegex = new(@"^[^\.]+\.Controllers\.App\.?(.*)$", RegexOptions.Compiled);

    private readonly Type[] _requiredTypes = new[] { typeof(IPeerServiceConfiguration), typeof(IPeerStartup) };

    public PeerAppInfo FindAppLibrary()
    {
        DirectoryInfo? dirInfo = Directory.GetParent(AppContext.BaseDirectory);
        if (dirInfo == null)
        {
            throw new DirectoryNotFoundException("Unable to find bin directory."); // Should never happen
        }

        var peerLibraryPath = Assembly.GetExecutingAssembly().Location;
        List<Type> requiredControllers = GetRequiredControllers();

        foreach (string libraryPath in Directory.GetFileSystemEntries(dirInfo.FullName, "*.dll"))
        {
            if (libraryPath.Equals(peerLibraryPath, StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }

            var appTypes = GetAppTypes(libraryPath);
            if (TryGetAppInfo(appTypes, requiredControllers, out PeerAppInfo appInfo))
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

    private bool TryGetAppInfo(Type[] libraryTypes, List<Type> peerControllers, out PeerAppInfo appInfo)
    {
        PeerAppInfo result = new();
        int peerControllerCount = 0;

        foreach (Type type in libraryTypes)
        {
            Type? abstractType = _requiredTypes.FirstOrDefault(t => t.IsAssignableFrom(type));
            if (abstractType != null)
            {
                result.RequiredTypes[abstractType] = type;
                continue;
            }

            if (type.Namespace == null)
            {
                continue;
            }

            var appNsMatch = _appControllerNsRegex.Match(type.Namespace);
            if (appNsMatch.Success)
            {
                var actions = MapAppControllerActions(type, appNsMatch.Groups[1].Value, "/app");
                if (actions.Count > 0)
                {
                    result.Controllers.Add(type);
                    result.RoutingMap.ConcatDictionary(actions);
                }
                continue;
            }

            var peerNsMatch = _peerControllerNsRegex.Match(type.Namespace);
            if (peerNsMatch.Success)
            {
                var controller = peerControllers.FirstOrDefault(t => t.IsAssignableFrom(type));
                if (controller == null)
                {
                    continue;
                }

                peerControllerCount++;

                var actions = MapAppControllerActions(type, peerNsMatch.Groups[1].Value, "/peer");
                result.Controllers.Add(type);
                result.RoutingMap.ConcatDictionary(actions);

                continue;
            }
        }

        if (result.RequiredTypes.Count != _requiredTypes.Length || peerControllerCount != peerControllers.Count)
        {
            appInfo = new PeerAppInfo();
            return false;
        }

        appInfo = result;
        return true;
    }

    private List<Type> GetRequiredControllers()
    {
        List<Type> result = new();

        foreach (Type type in Assembly.GetExecutingAssembly().GetExportedTypes())
        {
            if (type.Namespace == null || !type.IsInterface)
            {
                continue;
            }

            var nsMatch = _controllerInterfaceNsRegex.Match(type.Namespace);
            if (nsMatch.Success)
            {
                result.Add(type);
            }
        }

        return result;
    }

    private Dictionary<string, ControllerActionInfo> MapAppControllerActions(Type controllerType, string namespaceMatch, string pathPrefix)
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
            action => $"{pathPrefix}/{controllerPath}/{controllerType.Name}/{action?.MethodInfo.Name}".ToLower(),
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

using CoreLibrary.Interfaces;
using System.Reflection;

namespace PeerLibrary.Configuration;

/// <summary>
/// It's the bin folder inspector!... Bin-spector! Get it? Haha.
/// </summary>
internal class Binspector
{
    private readonly Type[] _requiredTypes = new[] { typeof(IPeerServiceConfiguration), typeof(IPeerRouting), typeof(IPeerStartup) };

    public Dictionary<Type, Type> FindAppLibrary()
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
            if (!TryGetAllRequiredTypes(appTypes, out Dictionary<Type, Type> allRequiredTypes))
            {
                continue;
            }

            return allRequiredTypes;
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

    private bool TryGetAllRequiredTypes(Type[] libraryTypes, out Dictionary<Type, Type> allRequiredTypes)
    {
        Queue<Type> requiredTypeQ = new(_requiredTypes);

        Dictionary<Type, Type> result = new();
        while (requiredTypeQ.TryDequeue(out var abstractType))
        {
            Type? concreteType = libraryTypes.FirstOrDefault(t => abstractType.IsAssignableFrom(t));
            if (concreteType == null)
            {
                allRequiredTypes = new Dictionary<Type, Type>();
                return false;
            }

            result[abstractType] = concreteType;
        }

        allRequiredTypes = result;
        return true;
    }
}

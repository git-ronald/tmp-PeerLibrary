using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;

namespace PeerLibrary.PeerApp;

internal class PeerRouting //: IPeerRouting
{
    private readonly Dictionary<string, ControllerActionInfo> _routingMap;
    private readonly IServiceProvider _serviceProvider;

    public PeerRouting(Dictionary<string, ControllerActionInfo> routingMap, IServiceProvider serviceProvider)
    {
        _routingMap = routingMap;
        _serviceProvider = serviceProvider;
    }

    public async Task<(bool Found, object? Result)> CallControllerAction(string path, JsonElement? data)
    {
        if (!_routingMap.TryGetValue(path.ToLower(), out ControllerActionInfo? action) || action == null)
        {
            return (false , null);
        }

        var controller = _serviceProvider.GetRequiredService(action.ControllerType);

        object?[] GetRequestParameters()
        {
            if (action?.ArgumentType == null)
            {
                return Array.Empty<object>();
            }

            var argumentValue = data?.Deserialize(action.ArgumentType);
            return new object?[] { argumentValue };
        }

        var returnedTask = (Task?)action.MethodInfo.Invoke(controller, GetRequestParameters());
        if (action.ReturnType == null || returnedTask == null)
        {
            return (true, null);
        }

        await returnedTask;
        object? returnValue = action.ReturnType.GetProperty("Result")?.GetValue(returnedTask);
        return (true, returnValue);

        //if (action.ArgumentType != null && data.HasValue)
        //{
        //    var requestArgument = data.Value.Deserialize(action.ArgumentType);
        //    var returnedTask = (Task?)action.MethodInfo.Invoke(controller, new object?[] { requestArgument });

        //    if (returnedTask == null)
        //    {
        //        return new ActionCallResult(true, action.ReturnType != null, null);
        //    }

        //    await returnedTask;

        //    if (action.ReturnType == null)
        //    {
        //        return new ActionCallResult(true, false,null);
        //    }

        //    var returnValue = action.ReturnType.GetProperty("Result")?.GetValue(returnedTask);

        //    return new ActionCallResult(true, action.ReturnType != null, returnValue);
        //}
        //else
        //{
        //    object? returnValue = action.MethodInfo.Invoke(controller, Array.Empty<object>()); // TODO NOW: this is wrong since it's a Task. See block above.
        //    return new ActionCallResult(true, action.ReturnType != null, returnValue);
        //}
    }
}

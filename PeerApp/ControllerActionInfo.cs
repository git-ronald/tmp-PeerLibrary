using System.Reflection;

namespace PeerLibrary.PeerApp
{
    internal class ControllerActionInfo
    {
        public ControllerActionInfo()
        {
        }

        public ControllerActionInfo(MethodInfo method, Type? argumentType, Type? returnType)
        {
            ControllerType = method.DeclaringType ?? typeof(object);
            MethodInfo = method;
            ArgumentType = argumentType;
            ReturnType = returnType;
        }

        public Type ControllerType { get; } = typeof(object);
        public MethodInfo MethodInfo { get; } = typeof(object).GetMethods()[0];
        public Type? ArgumentType { get; }
        public Type? ReturnType { get; }
    }
}

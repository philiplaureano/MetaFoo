using Optional;

namespace MetaFoo.Core.Reflection
{
    public static class MethodInvokerExtensions
    {
        public static Option<object> Invoke(this IMethodInvoker invoker, params object[] arguments)
        {
            return invoker.Invoke(Option.None<string>(), arguments);
        }
        
        public static Option<object> Invoke(this IMethodInvoker invoker, string methodName, params object[] arguments)
        {
            return invoker.Invoke(Option.Some(methodName), arguments);
        }
    }
}
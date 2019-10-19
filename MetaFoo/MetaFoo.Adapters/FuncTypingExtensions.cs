using System;
using System.Linq;
using System.Reflection;
using LightInject.Interception;
using MetaFoo.Reflection;
using Optional;
using Optional.Unsafe;

namespace MetaFoo.Adapters
{
    public static class FuncTypingExtensions
    {
        public static Option<T> CreateDelegate<T>(this IMethodInvoker invoker)
            where T : MulticastDelegate
        {
            return CreateDelegate<T>(new MethodInvokeAdapter(invoker));
        }

        public static Option<T> CreateDelegate<T>(this IInterceptor interceptor)
            where T : MulticastDelegate
        {
            // Route the interface call to the interceptor
            var interfaceType = InterfaceTypeBuilder.CreateInterfaceTypeFrom<T>();

            if (!interfaceType.HasValue)
                return Option.None<T>();

            var typeToProxy = interfaceType.ValueOrFailure();
            var definition = new ProxyDefinition(typeToProxy, () => null);
            definition.Implement(() => interceptor);

            // Call the proxy whenever the delegate is called
            var proxy = definition.CreateProxy();

            var targetMethod = typeToProxy.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single();

            var delegateInstance =
                Activator.CreateInstance(typeof(T), new[] {proxy, targetMethod.MethodHandle.GetFunctionPointer()});

            return delegateInstance is T result ? Option.Some(result) : Option.None<T>();
        }
    }
}
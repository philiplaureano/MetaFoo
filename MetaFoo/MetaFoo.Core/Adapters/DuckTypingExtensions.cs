using System;
using LightInject.Interception;
using MetaFoo.Core.Reflection;

namespace MetaFoo.Core.Adapters
{
    public static class DuckTypingExtensions
    {
        public static T CreateDuck<T>(this object target)
            where T : class
        {
            if (target is IMethodInvoker invoker)
                return invoker.CreateDuck<T>();
            
            var definition = new ProxyDefinition(typeof(T), () => null);
            definition.Implement(() => new DuckTypeInterceptor(target));
            return definition.CreateProxy<T>();
        }

        public static TInterface CreateDuck<TInterface>(this MulticastDelegate targetDelegate, string targetMethodName)
            where TInterface : class
        {
            var adapter = new DelegateAdapter<TInterface>(targetDelegate, targetMethodName);
            return adapter.CreateDuck<TInterface>();
        }

        public static T CreateDuck<T>(this IInterceptor interceptor)
        {
            var definition = new ProxyDefinition(typeof(T), () => null);

            definition.Implement(() => interceptor);
            return definition.CreateProxy<T>();
        }

        public static T CreateDuck<T>(this IMethodInvoker target)
        {
            var adapter = new MethodInvokeAdapter(target);
            return adapter.CreateDuck<T>();
        }

        public static object CreateDuck(this IInterceptor interceptor, Type proxyType)
        {
            var definition = new ProxyDefinition(proxyType, () => null);

            definition.Implement(() => interceptor);
            return definition.CreateProxy();
        }
        
        public static object CreateDuck(this IMethodInvoker target, Type proxyType)
        {
            var adapter = new MethodInvokeAdapter(target);
            return adapter.CreateDuck(proxyType);
        }
    }
}
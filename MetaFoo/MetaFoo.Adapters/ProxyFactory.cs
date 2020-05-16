using System;
using LightInject.Interception;

namespace MetaFoo.Adapters
{
    public static class ProxyFactory
    {
        public static T CreateProxy<T>(this ProxyDefinition definition)
        {
            var builder = new ProxyBuilder();
            var proxyType = builder.GetProxyType(definition);

            return (T) Activator.CreateInstance(proxyType);
        }

        public static object CreateProxy(this ProxyDefinition definition)
        {
            var builder = new ProxyBuilder();
            var proxyType = builder.GetProxyType(definition);

            return Activator.CreateInstance(proxyType);
        }
    }
}
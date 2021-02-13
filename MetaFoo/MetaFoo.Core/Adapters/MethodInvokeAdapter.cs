using LightInject.Interception;
using MetaFoo.Core.Reflection;
using Optional.Unsafe;

namespace MetaFoo.Core.Adapters
{
    internal class MethodInvokeAdapter : IInterceptor
    {
        private readonly IMethodInvoker _invoker;

        public MethodInvokeAdapter(IMethodInvoker invoker)
        {
            _invoker = invoker;
        }

        public object Invoke(IInvocationInfo invocationInfo)
        {
            var result = _invoker.Invoke(invocationInfo.TargetMethod.Name, invocationInfo.Arguments);

            return result.ValueOrDefault();
        }
    }
}
using System;
using LightInject.Interception;
using MetaFoo.Core.Reflection;
using Optional;
using Optional.Unsafe;

namespace MetaFoo.Core.Adapters
{
    public abstract class LateBoundInterceptor : IInterceptor
    {
        public object Invoke(IInvocationInfo invocationInfo)
        {
            if (invocationInfo == null)
                throw new ArgumentNullException(nameof(invocationInfo));

            var invoker = GetInvoker(invocationInfo);

            if (!invoker.HasValue)
                throw new NotImplementedException();

            var methodInvoker = invoker.ValueOrDefault();
            var methodName = invocationInfo.Method?.Name ?? string.Empty;

            var returnValue = methodInvoker.Invoke(methodName, invocationInfo.Arguments);
            return returnValue.HasValue ? returnValue.ValueOrDefault() : null;
        }

        protected abstract Option<IMethodInvoker> GetInvoker(IInvocationInfo invocationInfo);
    }
}
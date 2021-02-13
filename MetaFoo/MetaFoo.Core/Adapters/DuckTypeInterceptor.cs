using LightInject.Interception;
using MetaFoo.Core.Reflection;
using Optional;

namespace MetaFoo.Core.Adapters
{
    public class DuckTypeInterceptor : LateBoundInterceptor
    {
        private readonly IMethodInvoker _invoker;

        public DuckTypeInterceptor(object target) : this(new MethodInvoker(target))
        {
        }

        public DuckTypeInterceptor(IMethodInvoker methodInvoker)
        {
            _invoker = methodInvoker;
        }

        protected override Option<IMethodInvoker> GetInvoker(IInvocationInfo invocationInfo)
        {
            return Option.Some(_invoker);
        }
    }
}
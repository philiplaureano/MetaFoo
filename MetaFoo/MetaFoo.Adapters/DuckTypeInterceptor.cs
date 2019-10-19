using LightInject.Interception;
using MetaFoo.Reflection;
using Optional;

namespace MetaFoo.Adapters
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
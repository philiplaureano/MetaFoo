using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using MetaFoo.Core.Reflection;
using Optional;
using Optional.Unsafe;

namespace MetaFoo.Core.Dynamic
{
    internal class DelegateInvoker : IMethodInvoker
    {
        private ConcurrentDictionary<string, ConcurrentBag<MulticastDelegate>> _methods;

        public DelegateInvoker(ConcurrentDictionary<string, ConcurrentBag<MulticastDelegate>> methods)
        {
            _methods = methods;
        }

        public Option<object> Invoke(Option<string> methodName, params object[] arguments)
        {
            var candidateDelegates = methodName.HasValue && _methods.ContainsKey(methodName.ValueOrFailure())
                ? _methods[methodName.ValueOrFailure()].ToArray()
                : Enumerable.Empty<MulticastDelegate>();

            var delegatesByMethod = candidateDelegates.ToDictionary(d => d.Method);
            
            var finder = new MethodBaseFinder<MethodInfo>();
            
            // Note: The method name comparisons between delegates and the target method name have been skipped
            // because they will never match
            var bestMatch = finder.GetBestMatch(delegatesByMethod.Keys, string.Empty, arguments);
            if (!bestMatch.HasValue)
                return Option.None<object>();

            
            var targetMethod = bestMatch.ValueOrFailure();
            var targetDelegate = delegatesByMethod[targetMethod];

            var returnValue = targetMethod.Invoke(targetDelegate.Target, arguments);
            return Option.Some(returnValue);
        }
    }
}
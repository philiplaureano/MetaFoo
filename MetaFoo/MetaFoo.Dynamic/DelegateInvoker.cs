using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using MetaFoo.Reflection;
using Optional;
using Optional.Unsafe;

namespace MetaFoo.Dynamic
{
    internal class DelegateInvoker : IMethodInvoker
    {
        private ConcurrentDictionary<string, ConcurrentBag<MulticastDelegate>> _methods;

        public DelegateInvoker(ConcurrentDictionary<string, ConcurrentBag<MulticastDelegate>> methods)
        {
            _methods = methods;
        }

        public Option<object> Invoke(string methodName, params object[] arguments)
        {
            var candidateDelegates = _methods.ContainsKey(methodName)
                ? _methods[methodName].ToArray()
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
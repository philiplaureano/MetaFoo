using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MetaFoo.Core.Reflection;
using Optional;
using Optional.Unsafe;

namespace MetaFoo.Core.Dynamic
{
    internal class DelegateInvoker : IMethodInvoker
    {
        private readonly MetaObject _owner;
        private readonly ConcurrentDictionary<string, ConcurrentBag<MulticastDelegate>> _methods;

        public DelegateInvoker(MetaObject owner, ConcurrentDictionary<string, ConcurrentBag<MulticastDelegate>> methods)
        {
            _owner = owner;
            _methods = methods;
        }

        public Option<object> Invoke(Option<string> methodName, params object[] arguments)
        {
            var candidateDelegates = (methodName.HasValue && _methods.ContainsKey(methodName.ValueOrFailure())
                ? _methods[methodName.ValueOrFailure()].ToArray()
                : Enumerable.Empty<MulticastDelegate>()).ToArray();

            var delegatesByMethod = candidateDelegates.ToDictionary(d => d.Method);
            var candidateMethods = candidateDelegates.Select(d => d.Method).ToArray();

            var finder = new MethodBaseFinder<MethodInfo>();

            // Note: The method name comparisons between delegates and the target method name have been skipped
            // because they will never match
            var bestMatch = finder.GetBestMatch(delegatesByMethod.Keys, string.Empty, arguments);

            // Fall back to a method with a MetaObject parameter as the first optional parameter, if it exists
            var methodArgs = new List<object>(arguments);
            if (!bestMatch.HasValue && candidateMethods.Any(m => m.IsFallbackMethod()))
            {
                var modifiedArgs = new List<object> {_owner};
                modifiedArgs.AddRange(methodArgs);

                bestMatch = finder.GetBestMatch(delegatesByMethod.Keys, string.Empty, modifiedArgs);

                // Swap the method args and prepend
                // the owner instance if we have a match
                if (bestMatch.HasValue)
                    methodArgs = modifiedArgs;
            }

            if (!bestMatch.HasValue)
                return Option.None<object>();

            var targetMethod = bestMatch.ValueOrFailure();
            var targetDelegate = delegatesByMethod[targetMethod];

            var returnValue = targetMethod.Invoke(targetDelegate.Target, methodArgs.ToArray());

            return Option.Some(returnValue);
        }
    }
}
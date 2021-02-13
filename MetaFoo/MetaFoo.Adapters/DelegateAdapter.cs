using System;
using System.Linq;
using System.Reflection;
using LightInject.Interception;
using MetaFoo.Reflection;
using Optional;

namespace MetaFoo.Adapters
{
    internal class DelegateAdapter<TInterface> : IInterceptor
    {
        private MulticastDelegate _targetDelegate;
        private readonly string _targetMethodName;
        private readonly MethodInfo _targetDelegateMethodSignature;

        public DelegateAdapter(MulticastDelegate targetDelegate, string targetMethodName)
        {
            _targetDelegate = targetDelegate;
            _targetMethodName = targetMethodName;

            _targetDelegateMethodSignature = _targetDelegate.Method;
        }

        public object Invoke(IInvocationInfo invocationInfo)
        {
            var targetMethodName = _targetMethodName;
            var candidateMethods = typeof(TInterface).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == targetMethodName).ToArray();

            var finder = new MethodBaseFinder<MethodInfo>();

            var bestMatch = finder.GetBestMatch(candidateMethods,
                new MethodFinderContext(Option.Some(targetMethodName), invocationInfo.Arguments));

            if (!bestMatch.HasValue)
                throw new NotImplementedException($"Invocation error: Unable to find a method that is compatible with the '{targetMethodName}' method");

            // Match the signatures
            var matchingSignature = finder.GetBestMatch(new[] {_targetDelegateMethodSignature},
                new MethodFinderContext(Option.None<string>(), invocationInfo.Arguments));

            if(!matchingSignature.HasValue)
                throw new NotImplementedException($"Invocation error: Unable to find a method that is compatible with the '{targetMethodName}' method");

            var result = _targetDelegateMethodSignature.Invoke(_targetDelegate.Target, invocationInfo.Arguments);
            
            return result;
        }
    }
}
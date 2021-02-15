using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using MetaFoo.Core.Adapters;
using MetaFoo.Core.Reflection;
using Optional;
using Optional.Collections;
using Optional.Unsafe;

namespace MetaFoo.Core.Dynamic
{
    public class MetaObject : DynamicObject, IMethodInvoker
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<MulticastDelegate>> _methods =
            new();

        public MetaObject()
        {
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _methods.Keys;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            var targetType = binder.ReturnType;

            result = null;

            // Only interface types can be duck typed
            if (!targetType.IsInterface)
                return false;

            var invoker = new DelegateInvoker(this, _methods);
            var proxy = invoker.CreateDuck(targetType);
            result = proxy;

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var memberName = binder.Name;

            // If the member name does not start with the 'get_XYZ' prefix but the client is calling it as
            // value = foo.XYZ, then map the call to a getter call
            if (!memberName.StartsWith("get_") && _methods.ContainsKey($"get_{memberName}"))
            {
                var getterDelegates = _methods[$"get_{memberName}"];
                var delegatesByMethod = getterDelegates.ToDictionary(d => d.Method);

                // Use the first compatible getter
                var getterMethod = delegatesByMethod.Keys.Where(m =>
                    binder.ReturnType.IsAssignableFrom(m.ReturnType)
                    && !m.GetParameters().Any()).FirstOrNone();

                if (getterMethod.HasValue)
                {
                    var targetMethod = getterMethod.ValueOrFailure();
                    var targetDelegate = delegatesByMethod[targetMethod];

                    result = targetMethod.Invoke(targetDelegate.Target, new object[0]);
                    return true;
                }
            }

            var methods = _methods.ContainsKey(memberName) ? _methods[memberName].ToArray() : new MulticastDelegate[0];

            result = new BoundDynamicObject(methods);

            return true;
        }


        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = null;

            if (!args.Any())
                return false;

            if (args[0] is string methodName)
            {
                var remainingArgs = new List<object>();
                for (var i = 1; i < args.Length; i++)
                {
                    remainingArgs.Add(args[i]);
                }

                return DoInvoke(remainingArgs.ToArray(), out result, Option.Some(methodName));
            }

            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var methodName = binder.Name;
            return DoInvoke(args, out result, Option.Some(methodName));
        }

        private bool DoInvoke(object[] args, out object result, Option<string> methodName)
        {
            var candidateDelegates = (methodName.HasValue && _methods.ContainsKey(methodName.ValueOrFailure())
                ? _methods[methodName.ValueOrFailure()].ToArray()
                : Enumerable.Empty<MulticastDelegate>()).ToArray();

            var delegatesByMethod = candidateDelegates.ToDictionary(d => d.Method);
            var candidateMethods = candidateDelegates.Select(d => d.Method).ToArray();

            var methodArgs = new List<object>(args);
            result = null;

            if (!TryResolve(methodName, args, candidateMethods, methodArgs, out var bestMatch))
                return false;

            var bestMatchingMethod = bestMatch.ValueOrFailure();
            if (!delegatesByMethod.ContainsKey(bestMatchingMethod))
                return false;

            var targetDelegate = delegatesByMethod[bestMatchingMethod];
            var returnValue = bestMatchingMethod.Invoke(targetDelegate.Target, methodArgs.ToArray());

            result = returnValue;
            return true;
        }

        private bool TryResolve(Option<string> methodName, object[] args, MethodInfo[] candidateMethods,
            List<object> methodArgs,
            out Option<MethodInfo> bestMatch)
        {
            var finder = new MethodBaseFinder<MethodInfo>();
            bestMatch = finder.GetBestMatch(candidateMethods, new MethodFinderContext(methodName, args));
            if (bestMatch.HasValue)
                return true;

            // Find the closest match if we can't match the method name
            bestMatch = finder.GetBestMatch(candidateMethods, new MethodFinderContext(Option.None<string>(), args));
            if (bestMatch.HasValue)
                return true;

            // If the search fails, check if there are any methods that take a MetaObject as a first optional parameter
            if (candidateMethods.Any(m => m.IsFallbackMethod()))
            {
                // Insert the 'this' parameter into the list of args and see if we
                // can find a match
                methodArgs.Clear();
                methodArgs.Add(this);
                methodArgs.AddRange(args);
            }

            if (!candidateMethods.Any(m => m.IsFallbackMethod()))
                return false;

            bestMatch = finder.GetBestMatch(candidateMethods,
                new MethodFinderContext(Option.None<string>(), methodArgs));

            return bestMatch.HasValue;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var memberName = binder.Name;

            if (CallPropertySetter(memberName, value))
                return true;

            // If there is no existing member and the value is 
            // a delegate, it means that the caller wants to add a new property
            if (string.IsNullOrWhiteSpace(memberName) || value is not MulticastDelegate multicastDelegate)
                return false;

            if (!_methods.ContainsKey(memberName))
                _methods[memberName] = new ConcurrentBag<MulticastDelegate>();

            _methods[memberName].Add(multicastDelegate);

            return true;
        }

        private bool CallPropertySetter(string memberName, object value)
        {
            // If the member name does not start with the 'set_XYZ' prefix but the client is calling it as
            // foo.XYZ = value, then map the call to a setter call
            if (memberName.StartsWith("set_") || !_methods.ContainsKey($"set_{memberName}"))
                return false;

            var setterDelegates = _methods.GetValueOrNone($"set_{memberName}");
            if (!setterDelegates.HasValue)
                return false;

            var finder = new MethodBaseFinder<MethodInfo>();
            var delegatesByMethod = setterDelegates.ValueOrFailure().ToDictionary(d => d.Method);
            var bestMatch = finder.GetBestMatch(delegatesByMethod.Keys,
                string.Empty, new[] {value});

            if (!bestMatch.HasValue)
                return false;

            var targetMethod = bestMatch.ValueOrFailure();
            var targetDelegate = delegatesByMethod[targetMethod];

            // Call the setter
            targetMethod.Invoke(targetDelegate.Target, new[] {value});
            return true;
        }

        public Option<object> Invoke(Option<string> methodName, params object[] arguments)
        {
            return DoInvoke(arguments, out var result, methodName) ? Option.Some(result) : Option.None<object>();
        }

        public void AddMethod<TDelegate>(string methodName, TDelegate methodImplementation)
            where TDelegate : MulticastDelegate
        {
            if (!_methods.ContainsKey(methodName))
                _methods[methodName] = new ConcurrentBag<MulticastDelegate>();

            var targetCollection = _methods[methodName];
            targetCollection.Add(methodImplementation);
        }

        public bool LooksLike<T>()
            where T : class
        {
            var methodsToMatch = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance).ToArray();

            var finder = new MethodBaseFinder<MethodBase>();
            foreach (var method in methodsToMatch)
            {
                // Match the method name
                var methodName = method.Name;
                if (!_methods.ContainsKey(methodName))
                    return false;

                var candidateMethods = _methods[methodName]
                    .Select(currentDelegate => currentDelegate.Method).ToArray();

                // Match the method signature
                var methodArgTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                IMethodFinderContext CreateContext(MethodInfo targetMethod, IEnumerable<Type> argTypes)
                {
                    return targetMethod.ReturnType == typeof(void)
                        ? new MethodFinderContext(Option.None<string>(), argTypes)
                        : new MethodFinderContext(Option.None<string>(), argTypes,
                            Option.Some(targetMethod.ReturnType));
                }

                var methodFinderContext = CreateContext(method, methodArgTypes);
                var hasCompatibleMethod = finder.HasMatchingMethods(candidateMethods,
                    methodFinderContext);

                // Fall back to the special methods with a MetaObject as the first parameter in case
                // there's a method that requires access to the MetaObject in question
                if (!hasCompatibleMethod && candidateMethods.Any(m => m.IsFallbackMethod()))
                {
                    var modifiedArgTypes = new List<Type> {typeof(MetaObject)};
                    modifiedArgTypes.AddRange(methodArgTypes);

                    var fallbackContext = CreateContext(method, modifiedArgTypes);
                    hasCompatibleMethod |= finder.HasMatchingMethods(candidateMethods, fallbackContext);
                }

                if (!hasCompatibleMethod)
                    return false;
            }

            return true;
        }
    }
}
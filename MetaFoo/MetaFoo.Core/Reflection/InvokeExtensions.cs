using System;
using System.Reflection;
using Optional;
using Optional.Unsafe;

namespace MetaFoo.Core.Reflection
{
    public static class InvokeExtensions
    {
        public static Option<object> Invoke(this Option<MethodInfo> optionalMethod, Option<object> optionalTargetInstance,
            object[] args)
        {
            if (!optionalMethod.HasValue)
                return Option.None<object>();

            var targetInstance = optionalTargetInstance.ValueOrDefault();
            var targetMethod = optionalMethod.ValueOrFailure();
            var result = targetMethod.Invoke(targetInstance, args);

            return targetMethod.ReturnType == typeof(void) ? Option.None<object>() : Option.Some(result);
        }

        public static Option<object> InvokeStatic(this Type targetType, string methodName, params object[] args)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            var methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var context = new MethodFinderContext(Option.Some(methodName), args);
            var finder = new MethodBaseFinder<MethodInfo>();

            var matchingMethod = finder.GetBestMatch(methods, context);
            if (!matchingMethod.HasValue)
                throw new MethodNotFoundException(methodName, args);

            return matchingMethod.Invoke(Option.None<object>(), args);
        }
    }
}
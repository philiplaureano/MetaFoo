using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LinFu.Loaders;
using Optional;

namespace MetaFoo.Core.Reflection
{
    public class MethodInvoker : IMethodInvoker
    {
        private readonly List<MethodInfo> _extensionMethodPool = new List<MethodInfo>();
        private readonly List<MethodInfo> _instanceMethodPool = new List<MethodInfo>();
        private readonly object _target;

        public MethodInvoker(object target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));

            var targetType = target.GetType();
            var instanceMethods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            _instanceMethodPool.AddRange(instanceMethods);
        }

        public void AddExtensionMethodsFrom(Assembly extensionAssembly)
        {
            var extensionAttributeType = typeof(ExtensionAttribute);

            // Scan the target assembly for extension methods that pertain to the current type
            var typeExtractor = new TypeExtractor();
            var loadedTypes = typeExtractor.GetTypes(extensionAssembly);

            // Add every extension method inside that assembly
            var bindingFlags = BindingFlags.Public | BindingFlags.Static;
            var extensionMethods = loadedTypes.SelectMany(t => t.GetMethods(bindingFlags))
                .Where(m => m.GetCustomAttributes(extensionAttributeType, false).Any());

            _extensionMethodPool.AddRange(extensionMethods);
        }

        public void AddExtensionMethodsFrom(Type extensionClassType)
        {
            var extensionAttributeType = typeof(ExtensionAttribute);
            var matchingMethods = extensionClassType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.GetCustomAttributes(extensionAttributeType, false).Any()).ToArray();

            _extensionMethodPool.AddRange(matchingMethods);
        }

        public Option<object> Invoke(Option<string> methodName, params object[] arguments)
        {
            Option<MethodInfo> bestMatch;

            // Search the instance methods
            var finder = new MethodBaseFinder<MethodInfo>();
            bestMatch = finder.GetBestMatch(_instanceMethodPool, new MethodFinderContext(methodName, arguments));

            if (bestMatch.HasValue)
                return bestMatch.Invoke(Option.Some(_target), arguments);

            // Fall back to the extension methods if it isn't found
            var adjustedArguments = new List<object> {_target};
            adjustedArguments.AddRange(arguments);

            var context = new MethodFinderContext(methodName, adjustedArguments);
            bestMatch = finder.GetBestMatch(_extensionMethodPool, context);
            
            return bestMatch.Invoke(Option.None<object>(), adjustedArguments.ToArray());
        }
    }
}
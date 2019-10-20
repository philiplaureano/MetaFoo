using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using MetaFoo.Reflection;
using Optional.Unsafe;

namespace MetaFoo.Dynamic
{
    internal class BoundDynamicObject : System.Dynamic.DynamicObject
    {
        private readonly IEnumerable<MulticastDelegate> _delegates;

        internal BoundDynamicObject(IEnumerable<MulticastDelegate> delegates)
        {
            _delegates = delegates;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            var finder = new MethodFinder<MethodInfo>();
            var context = new MethodFinderContext(string.Empty, args);

            var delegatesByMethod = _delegates.ToDictionary(d => d.Method);
            var methods = delegatesByMethod.Keys;

            result = null;

            var bestMatch = finder.GetBestMatch(methods, context);
            if (!bestMatch.HasValue)
                return false;

            var targetMethod = bestMatch.ValueOrFailure();
            var targetDelegate = delegatesByMethod[targetMethod];
            result = targetMethod.Invoke(targetDelegate.Target, args);

            return true;
        }
    }
}
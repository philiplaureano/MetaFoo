using System.Collections.Generic;
using Optional;

namespace MetaFoo.Reflection
{
    public class MethodFinderContext : IMethodFinderContext
    {
        public MethodFinderContext(Option<string> methodName, IEnumerable<object> arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
        }

        public Option<string> MethodName { get; }
        public IEnumerable<object> Arguments { get; }
    }
}
using System.Collections.Generic;

namespace MetaFoo.Reflection
{
    public class MethodFinderContext : IMethodFinderContext
    {
        public MethodFinderContext(string methodName, IEnumerable<object> arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
        }

        public string MethodName { get; }
        public IEnumerable<object> Arguments { get; }
    }
}
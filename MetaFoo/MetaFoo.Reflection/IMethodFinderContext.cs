using System.Collections.Generic;

namespace MetaFoo.Reflection
{
    public interface IMethodFinderContext
    {
        string MethodName { get; }

        IEnumerable<object> Arguments { get; }
    }
}
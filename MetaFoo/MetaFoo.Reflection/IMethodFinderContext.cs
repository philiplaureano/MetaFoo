using System.Collections.Generic;
using Optional;

namespace MetaFoo.Reflection
{
    public interface IMethodFinderContext
    {
        Option<string> MethodName { get; }

        IEnumerable<object> Arguments { get; }
    }
}
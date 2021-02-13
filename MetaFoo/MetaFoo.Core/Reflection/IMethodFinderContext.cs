using System.Collections.Generic;
using Optional;

namespace MetaFoo.Core.Reflection
{
    public interface IMethodFinderContext
    {
        Option<string> MethodName { get; }

        IEnumerable<object> Arguments { get; }
    }
}
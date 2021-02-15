using System;
using System.Collections.Generic;
using Optional;

namespace MetaFoo.Core.Reflection
{
    public interface IMethodFinderContext
    {
        Option<string> MethodName { get; }

        public IEnumerable<Option<Type>> ArgumentTypes { get; }
        Option<Type> ReturnType { get; }
    }
}
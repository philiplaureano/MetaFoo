using System.Collections.Generic;
using Optional;

namespace MetaFoo.Core.Reflection
{
    public interface IMethodFinderStrategy<TType, in TMethod>
    {
        bool IsAssignableFrom(TType sourceType, TType targetType);
        IEnumerable<TType> GetParameterTypes(TMethod method);
        Option<TType> GetType(object instance);

        string GetMethodName(TMethod method);
    }
}
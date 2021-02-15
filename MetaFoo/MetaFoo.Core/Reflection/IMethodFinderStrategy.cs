using System;
using System.Collections.Generic;
using Optional;

namespace MetaFoo.Core.Reflection
{
    public interface IMethodFinderStrategy<TType, in TMethod>
        where TType : Type
    {
        bool IsAssignableFrom(TType sourceType, TType targetType);
        IEnumerable<TType> GetParameterTypes(TMethod method);
        Option<TType> GetType(object instance);

        string GetMethodName(TMethod method);

        Option<TType> GetReturnType(TMethod method);
    }
}
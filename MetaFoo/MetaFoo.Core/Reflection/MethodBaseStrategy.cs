using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Optional;

namespace MetaFoo.Core.Reflection
{
    public class MethodBaseStrategy<TMethod> : IMethodFinderStrategy<Type, TMethod>
        where TMethod : MethodBase
    {
        public bool IsAssignableFrom(Type sourceType, Type targetType)
        {
            return sourceType.IsAssignableFrom(targetType);
        }

        public IEnumerable<Type> GetParameterTypes(TMethod method)
        {
            return method.GetParameters().Select(p => p.ParameterType);
        }

        public Option<Type> GetType(object instance)
        {
            return instance == null ? Option.None<Type>() : Option.Some(instance.GetType());
        }

        public string GetMethodName(TMethod method)
        {
            return method.Name;
        }
    }
}
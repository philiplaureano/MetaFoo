using System;
using System.Collections.Generic;
using System.Linq;
using Optional;

namespace MetaFoo.Core.Reflection
{
    public class MethodFinderContext : IMethodFinderContext
    {
        public MethodFinderContext(Option<string> methodName, IEnumerable<object> arguments)
        {
            MethodName = methodName;
            ArgumentTypes = arguments.Select(arg => arg == null ? Option.None<Type>() : Option.Some(arg.GetType()));

            ReturnType = Option.None<Type>();
        }

        public MethodFinderContext(Option<string> methodName, IEnumerable<Type> argumentTypes)
        {
            MethodName = methodName;
            ArgumentTypes = argumentTypes.Select(Option.Some).ToArray();
            ReturnType = Option.None<Type>();
        }
        
        public MethodFinderContext(Option<string> methodName, IEnumerable<Type> argumentTypes, Option<Type> returnType)
        {
            MethodName = methodName;
            ArgumentTypes = argumentTypes.Select(Option.Some).ToArray();
            ReturnType = returnType;
        }

        public Option<string> MethodName { get; }
        public IEnumerable<Option<Type>> ArgumentTypes { get; }
        public Option<Type> ReturnType { get; }
    }
}
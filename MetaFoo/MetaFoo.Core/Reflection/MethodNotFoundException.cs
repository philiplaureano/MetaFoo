using System;

namespace MetaFoo.Core.Reflection
{
    public class MethodNotFoundException : Exception
    {
        public MethodNotFoundException(string methodName, object[] arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
        }
        
        public string MethodName { get; }
        public object[] Arguments { get; }
    }
}
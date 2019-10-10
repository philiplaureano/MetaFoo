using System;
using System.Reflection;

namespace MetaFoo.Reflection
{
    public static class InvokeExtensions
    {
        public static object InvokeStatic(this Type targetType, string methodName, params object[] args)
        {
            if (targetType == null) 
                throw new ArgumentNullException(nameof(targetType));

            var methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var context = new MethodFinderContext(methodName, args);
            var finder = new MethodFinder<MethodInfo>();

            var matchingMethod = finder.GetBestMatch(methods, context);
            if(matchingMethod==null)
                throw new MethodNotFoundException(methodName,args);

            var result = matchingMethod.Invoke(null, args);
            
            return result;
        }
    }
}
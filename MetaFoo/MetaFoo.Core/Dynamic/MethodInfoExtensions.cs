using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace MetaFoo.Core.Dynamic
{
    internal static class MethodInfoExtensions
    {
        public static bool IsFallbackMethod(this MethodBase method)
        {
            var parameters = method.GetParameters();
            var hasDynamicObjectParameter = parameters.Length > 0 &&
                                            parameters.First().ParameterType
                                                .IsAssignableFrom(typeof(MetaObject));

            return hasDynamicObjectParameter;
        }
    }
}
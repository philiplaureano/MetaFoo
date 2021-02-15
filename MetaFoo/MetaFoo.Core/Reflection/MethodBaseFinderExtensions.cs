using System.Collections.Generic;
using System.Reflection;

namespace MetaFoo.Core.Reflection
{
    public static class MethodBaseFinderExtensions
    {
        public static bool HasMatchingMethods<TMethod>(this MethodBaseFinder<TMethod> finder,
            IEnumerable<TMethod> methods, IMethodFinderContext context)
            where TMethod : MethodBase
        {
            return finder.GetBestMatch(methods, context).HasValue;
        }
    }
}
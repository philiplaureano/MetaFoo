using System.Collections.Generic;
using System.Reflection;
using Optional;

namespace MetaFoo.Core.Reflection
{
    public static class MethodFinderExtensions
    {
        public static Option<TMethod> GetBestMatch<TMethod>(this IMethodFinder<TMethod> finder,
            IEnumerable<TMethod> methods, string methodName, IEnumerable<object> args) where TMethod : MethodBase
        {
            return finder.GetBestMatch(methods, new MethodFinderContext(Option.Some(methodName), args));
        }
    }
}
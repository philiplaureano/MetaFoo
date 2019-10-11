using System.Collections.Generic;
using System.Reflection;
using Optional;

namespace MetaFoo.Reflection
{
    public interface IMethodFinder<TMethod>
        where TMethod : MethodBase
    {
        Option<TMethod> GetBestMatch(IEnumerable<TMethod> methods, IMethodFinderContext finderContext);
    }
}
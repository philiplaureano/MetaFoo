using System.Collections.Generic;
using System.Reflection;

namespace MetaFoo.Reflection
{
    public interface IMethodFinder<TMethod>
        where TMethod : MethodBase
    {
        TMethod GetBestMatch(IEnumerable<TMethod> methods, IMethodFinderContext finderContext);
    }
}
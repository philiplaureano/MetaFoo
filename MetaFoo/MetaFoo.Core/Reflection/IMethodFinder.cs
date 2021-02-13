using System.Collections.Generic;
using Optional;

namespace MetaFoo.Core.Reflection
{
    public interface IMethodFinder<TMethod>
    {
        Option<TMethod> GetBestMatch(IEnumerable<TMethod> methods, IMethodFinderContext finderContext);
    }
}
using System;
using System.Reflection;

namespace MetaFoo.Core.Reflection
{
    public class MethodBaseFinder<TMethod> : MethodFinder<Type, TMethod>
        where TMethod : MethodBase
    {
        public MethodBaseFinder(double finderTolerance = 0.51) :
            base(new MethodBaseStrategy<TMethod>(), finderTolerance)
        {
        }
    }
}
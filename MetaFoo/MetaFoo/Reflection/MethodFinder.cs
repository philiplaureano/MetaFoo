using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LinFu.Finders;
using LinFu.Finders.Interfaces;

namespace MetaFoo.Reflection
{
    public class MethodFinder<TMethod> : IMethodFinder<TMethod>
        where TMethod : MethodBase
    {
        private readonly double _finderTolerance;

        public MethodFinder(double finderTolerance = .51)
        {
            _finderTolerance = finderTolerance;
        }

        public TMethod GetBestMatch(IEnumerable<TMethod> methods, IMethodFinderContext finderContext)
        {
            var methodName = finderContext.MethodName;

            var candidateMethods = (methods ?? new TMethod[0]);

            // Match the method name
            if (!string.IsNullOrEmpty(methodName))
                candidateMethods = candidateMethods.Where(m => m.Name == methodName);

            // Match the argument count
            var arguments = finderContext.Arguments.ToArray();
            var argumentCount = arguments.Length;

            candidateMethods = candidateMethods.Where(m => m.GetParameters().Length == argumentCount);

            // Find a compatible method signature
            bool HasCompatibleParameters(TMethod method, int position, IReadOnlyList<object> currentArguments)
            {
                var parameters = method.GetParameters().ToArray();
                if (currentArguments.Count != parameters.Length)
                    return false;

                var parameterType = parameters[position].ParameterType;
                var hasCompatibleParameterType = parameterType.IsAssignableFrom(currentArguments[position]?.GetType());

                return hasCompatibleParameterType;
            }

            // Exact parameter type matches will outweigh compatible method overloads
            bool HasExactParameterTypes(TMethod method, int position, IReadOnlyList<object> currentArguments)
            {
                var parameters = method.GetParameters().ToArray();
                if (currentArguments.Count != parameters.Length)
                    return false;

                var parameterType = parameters[position].ParameterType;
                return parameterType == currentArguments[position]?.GetType();
            }

            var fuzzyList = candidateMethods.AsFuzzyList();

            // Override the search results if there is only one match
            // and that match has compatible parameters
            if (fuzzyList.Count == 1)
            {
                var nextBestMatch = fuzzyList[0];

                var hasCompatibleParameters = true;
                for (var i = 0; i < arguments.Length; i++)
                {
                    hasCompatibleParameters &= HasCompatibleParameters(nextBestMatch.Item, i, arguments);
                }

                if (hasCompatibleParameters)
                    return nextBestMatch.Item;
            }

            // Otherwise, fall back to a weighted search against the other remaining methods
            for (var i = 0; i < arguments.Length; i++)
            {
                fuzzyList.AddCriteria(method => HasCompatibleParameters(method, i, arguments), CriteriaType.Critical);
                fuzzyList.AddCriteria(method => HasExactParameterTypes(method, i, arguments));
            }

            if (arguments.Length == 0)
                fuzzyList.AddCriteria(method => method.GetParameters().Length == 0);

            var bestMatch = fuzzyList.BestMatch(_finderTolerance);

            return bestMatch?.Item;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using LinFu.Finders;
using LinFu.Finders.Interfaces;
using Optional;
using Optional.Unsafe;

namespace MetaFoo.Reflection
{
    public class MethodFinder<TType, TMethod> : IMethodFinder<TMethod>
        where TMethod : class
    {
        private readonly double _finderTolerance;
        private readonly IMethodFinderStrategy<TType, TMethod> _methodFinderStrategy;

        public MethodFinder(IMethodFinderStrategy<TType, TMethod> methodFinderStrategy, double finderTolerance = .51)
        {
            _finderTolerance = finderTolerance;
            _methodFinderStrategy = methodFinderStrategy;
        }

        public Option<TMethod> GetBestMatch(IEnumerable<TMethod> methods, IMethodFinderContext finderContext)
        {
            var methodName = finderContext.MethodName;

            var candidateMethods = (methods ?? new TMethod[0]);

            // Match the method name
            if (methodName.HasValue && !string.IsNullOrEmpty(methodName.ValueOrDefault()))
                candidateMethods = candidateMethods.Where(m => _methodFinderStrategy.GetMethodName(m) == methodName.ValueOrFailure());

            // Match the argument count
            var arguments = finderContext.Arguments.ToArray();
            var argumentCount = arguments.Length;

            candidateMethods =
                candidateMethods.Where(m => _methodFinderStrategy.GetParameterTypes(m).Count() == argumentCount);

            // Find a compatible method signature
            bool HasCompatibleParameters(TMethod method, int position, IReadOnlyList<object> currentArguments)
            {
                var parameters = _methodFinderStrategy.GetParameterTypes(method).ToArray();
                if (currentArguments.Count != parameters.Length)
                    return false;

                var parameterType = parameters[position];
                var argumentType = _methodFinderStrategy.GetType(currentArguments[position]);
                if (!argumentType.HasValue)
                    return false;

                var hasCompatibleParameterType =
                    _methodFinderStrategy.IsAssignableFrom(parameterType, argumentType.ValueOrFailure());
                return hasCompatibleParameterType;
            }

            // Exact parameter type matches will outweigh compatible method overloads
            bool HasExactParameterTypes(TMethod method, int position, IReadOnlyList<object> currentArguments)
            {
                var parameters = _methodFinderStrategy.GetParameterTypes(method).ToArray();
                if (currentArguments.Count != parameters.Length)
                    return false;

                var parameterType = parameters[position];

                var argumentType = _methodFinderStrategy.GetType(currentArguments[position]);
                if (!argumentType.HasValue)
                    return false;

                return Equals(parameterType, argumentType.ValueOrFailure());
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
                    return Option.Some(nextBestMatch.Item);
            }

            // Otherwise, fall back to a weighted search against the other remaining methods
            for (var i = 0; i < arguments.Length; i++)
            {
                fuzzyList.AddCriteria(method => HasCompatibleParameters(method, i, arguments), CriteriaType.Critical);
                fuzzyList.AddCriteria(method => HasExactParameterTypes(method, i, arguments));
            }

            if (arguments.Length == 0)
                fuzzyList.AddCriteria(method => !_methodFinderStrategy.GetParameterTypes(method).Any());

            var bestMatch = fuzzyList.BestMatch(_finderTolerance);
            return bestMatch == null ? Option.None<TMethod>() : Option.Some(bestMatch?.Item);
        }
    }
}
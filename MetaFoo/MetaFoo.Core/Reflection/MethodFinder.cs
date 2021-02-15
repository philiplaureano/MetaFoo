using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LinFu.Finders;
using LinFu.Finders.Interfaces;
using Optional;
using Optional.Unsafe;

namespace MetaFoo.Core.Reflection
{
    public class MethodFinder<TType, TMethod> : IMethodFinder<TMethod>
        where TMethod : class
        where TType : Type

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

            var candidateMethods = (methods ?? Enumerable.Empty<TMethod>());

            // Match the method name
            if (methodName.HasValue && !string.IsNullOrEmpty(methodName.ValueOrDefault()))
                candidateMethods = candidateMethods.Where(m =>
                    _methodFinderStrategy.GetMethodName(m) == methodName.ValueOrFailure());

            // Match the argument count
            var argumentTypes = finderContext.ArgumentTypes.ToArray();
            var argumentCount = argumentTypes.Count();
            candidateMethods =
                candidateMethods.Where(m => _methodFinderStrategy.GetParameterTypes(m).Count() == argumentCount);

            // Find a compatible method signature
            bool HasCompatibleParameters(TMethod method, int position, IReadOnlyList<Option<Type>> currentArgumentTypes)
            {
                var parameters = _methodFinderStrategy.GetParameterTypes(method).ToArray();
                if (currentArgumentTypes.Count != parameters.Length)
                    return false;

                var parameterType = parameters[position];
                var argumentType = currentArgumentTypes[position];
                if (!argumentType.HasValue)
                    return false;

                var hasCompatibleParameterType =
                    parameterType.IsAssignableFrom(argumentType.ValueOrFailure());

                return hasCompatibleParameterType;
            }

            // Exact parameter type matches will outweigh compatible method overloads
            bool HasExactParameterTypes(TMethod method, int position, IReadOnlyList<Option<Type>> currentArguments)
            {
                var parameters = _methodFinderStrategy.GetParameterTypes(method).ToArray();
                if (currentArguments.Count != parameters.Length)
                    return false;

                var parameterType = parameters[position];

                var argumentType = currentArguments[position];
                return argumentType.HasValue && parameterType == argumentType.ValueOrFailure();
            }

            bool HasCompatibleReturnType(TMethod method, Type expectedReturnType)
            {
                var returnType = _methodFinderStrategy.GetReturnType(method);
                return returnType.HasValue && expectedReturnType.IsAssignableFrom(returnType.ValueOrFailure());
            }

            var fuzzyList = candidateMethods.AsFuzzyList();

            // Override the search results if there is only one match
            // and that match has compatible parameters
            var hasRequestedCompatibleMethodType = finderContext.ReturnType.HasValue;
            if (fuzzyList.Count == 1)
            {
                var nextBestMatch = fuzzyList[0];

                var hasCompatibleParameters = true;
                for (var i = 0; i < argumentTypes.Length; i++)
                {
                    hasCompatibleParameters &= HasCompatibleParameters(nextBestMatch.Item, i, argumentTypes);
                }

                // Match the return type if the caller requests a method return type match
                if (hasRequestedCompatibleMethodType && hasCompatibleParameters)
                    return HasCompatibleReturnType(nextBestMatch.Item, finderContext.ReturnType.ValueOrFailure())
                        ? Option.Some(nextBestMatch.Item)
                        : Option.None<TMethod>();

                if (hasCompatibleParameters)
                    return Option.Some(nextBestMatch.Item);
            }

            // Otherwise, fall back to a weighted search against the other remaining methods
            for (var i = 0; i < argumentTypes.Length; i++)
            {
                var currentIndex = i;
                fuzzyList.AddCriteria(method => HasCompatibleParameters(method, currentIndex, argumentTypes),
                    CriteriaType.Critical);
                fuzzyList.AddCriteria(method => HasExactParameterTypes(method, currentIndex, argumentTypes));
            }

            // Match the method return type
            if (hasRequestedCompatibleMethodType)
                fuzzyList.AddCriteria(
                    method => HasCompatibleReturnType(method, finderContext.ReturnType.ValueOrFailure()),
                    CriteriaType.Critical);

            if (argumentTypes.Length == 0)
                fuzzyList.AddCriteria(method => !_methodFinderStrategy.GetParameterTypes(method).Any());

            var bestMatch = fuzzyList.BestMatch(_finderTolerance);
            return bestMatch == null ? Option.None<TMethod>() : Option.Some(bestMatch?.Item);
        }
    }
}
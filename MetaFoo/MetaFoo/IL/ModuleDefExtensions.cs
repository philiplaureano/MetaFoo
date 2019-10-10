using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;

namespace MetaFoo.IL
{
    public static class ModuleDefExtensions
    {
        public static IMethod ImportConstructor<T>(this ModuleDef module,
            params Type[] constructorParameters)
        {
            return module.Import(typeof(T).GetConstructor(constructorParameters));
        }

        public static IMethod ImportMethod<T>(this ModuleDef module, string methodName,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                        BindingFlags.Static, params Type[] parameterTypes)
        {
            return module.ImportMethod(typeof(T), methodName, bindingFlags, parameterTypes ?? new Type[0]);
        }

        public static IMethod ImportMethod(this ModuleDef module, Type declaringType, string methodName,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                        BindingFlags.Static,
            params Type[] parameterTypes)
        {
            var candidateMethods = declaringType.GetMethods(bindingFlags).Where(m => m.Name == methodName);
            var currentParameterTypes = (parameterTypes ?? new Type[0]).ToArray();

            bool HasMatchingParameters(IReadOnlyCollection<Type> setA, IReadOnlyList<Type> setB)
            {
                if (setA?.Count != setB?.Count)
                    return false;

                return !setA.Where((t, i) => t != setB[i]).Any();
            }

            var targetMethod = candidateMethods.First(m =>
                HasMatchingParameters(currentParameterTypes,
                    m.GetParameters().Select(p => p.ParameterType).ToArray()));

            return module.Import(targetMethod);
        }
    }
}
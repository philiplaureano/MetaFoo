using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace MetaFoo.Core.IL
{
    public static class DelegateFactory
    {
        private static readonly ConcurrentDictionary<MethodInfo, Func<object, object[], object>> Entries =
            new ConcurrentDictionary<MethodInfo, Func<object, object[], object>>();

        public static Func<object, object[], object> CreateDelegateFrom(MethodInfo targetMethod)
        {
            // Reuse existing entries if possible
            if (Entries.ContainsKey(targetMethod))
                return Entries[targetMethod];

            var methodName = $"__AnonymousMethod__{Guid.NewGuid().ToString().Replace("-", string.Empty)}";
            var dynamicMethod = new DynamicMethod(methodName, typeof(object),
                new[] {typeof(object), typeof(object[])},
                typeof(string).Module);

            var body = dynamicMethod.GetILGenerator();

            var targetInstance = body.DeclareLocal(typeof(object));
            var arguments = body.DeclareLocal(typeof(object[]));

            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Stloc, targetInstance);

            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Stloc, arguments);

            if (!targetMethod.IsStatic)
                body.Emit(OpCodes.Ldloc, targetInstance);

            foreach (var parameter in targetMethod.GetParameters())
            {
                var parameterType = parameter.ParameterType;

                body.Emit(OpCodes.Ldloc, arguments);
                body.Emit(OpCodes.Ldc_I4, parameter.Position);
                body.Emit(OpCodes.Ldelem_Ref);

                if (parameterType.IsValueType)
                    body.Emit(OpCodes.Unbox_Any, parameterType);
            }

            if (targetMethod.IsStatic || !targetMethod.IsVirtual)
            {
                body.Emit(OpCodes.Call, targetMethod);
            }
            else
            {
                body.Emit(OpCodes.Callvirt, targetMethod);
            }

            var targetMethodReturnType = targetMethod.ReturnType;
            if (targetMethodReturnType == typeof(void))
                body.Emit(OpCodes.Ldnull);

            if (targetMethodReturnType.IsValueType && targetMethodReturnType != typeof(void))
                body.Emit(OpCodes.Box, targetMethodReturnType);

            body.Emit(OpCodes.Ret);

            var result = (Func<object, object[], object>) dynamicMethod.CreateDelegate(
                typeof(Func<object, object[], object>));

            Entries[targetMethod] = result;

            return result;
        }
    }
}
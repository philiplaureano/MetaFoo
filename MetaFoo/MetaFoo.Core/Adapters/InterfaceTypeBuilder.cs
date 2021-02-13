using System;
using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using Optional;
using Optional.Collections;
using Optional.Unsafe;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
using MethodImplAttributes = dnlib.DotNet.MethodImplAttributes;
using TypeAttributes = dnlib.DotNet.TypeAttributes;

namespace MetaFoo.Core.Adapters
{
    public static class InterfaceTypeBuilder
    {
        public static Option<Type> CreateInterfaceTypeFrom<TDelegate>()
            where TDelegate : MulticastDelegate
        {
            var targetMethod = typeof(TDelegate).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Invoke").FirstOrNone();

            if (!targetMethod.HasValue)
                return Option.None<Type>();

            var interfaceType = CreateInterfaceTypeFrom(targetMethod);
            return interfaceType;
        }

        public static Option<Type> CreateInterfaceTypeFrom(MethodInfo sourceMethod)
        {
            return CreateInterfaceTypeFrom(Option.Some(sourceMethod));
        }

        public static Option<Type> CreateInterfaceTypeFrom(Option<MethodInfo> sourceMethod)
        {
            if (!sourceMethod.HasValue)
                return Option.None<Type>();

            var methodInfo = sourceMethod.ValueOrFailure();
            var parameterTypes = methodInfo.GetParameters();
            var returnType = methodInfo.ReturnType;

            var typeId = Guid.NewGuid().ToString();
            var methodName = $"AnonymousMethod_{typeId}";

            var module = new ModuleDefUser($"anonymous_module_{typeId}.dll") {Kind = ModuleKind.Dll};

            var assembly = new AssemblyDefUser($"anonymous_assembly_{typeId}");
            assembly.Modules.Add(module);

            var interfaceType = new TypeDefUser($"IAnonymousInterface_{typeId}")
            {
                Attributes = TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Interface |
                             TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass
            };
            module.Types.Add(interfaceType);

            var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
                                   MethodAttributes.Abstract | MethodAttributes.Virtual;

            var methodImplAttributes = MethodImplAttributes.Managed | MethodImplAttributes.IL;

            var importedReturnType = module.ImportAsTypeSig(returnType);
            var importedParameterTypes = parameterTypes.Select(type => module.ImportAsTypeSig(type.ParameterType));
            var method = new MethodDefUser(methodName,
                MethodSig.CreateInstance(importedReturnType, importedParameterTypes.ToArray()),
                methodImplAttributes, methodAttributes);

            for (var paramNumber = 0; paramNumber < parameterTypes.Length; paramNumber++)
            {
                method.ParamDefs.Add(new ParamDefUser($"arg{paramNumber++}"));
            }

            interfaceType.Methods.Add(method);

            var stream = new MemoryStream();
            module.Write(stream);

            var loadedAssembly = Assembly.Load(stream.ToArray());

            return loadedAssembly.GetTypes().FirstOrNone();
        }
    }
}
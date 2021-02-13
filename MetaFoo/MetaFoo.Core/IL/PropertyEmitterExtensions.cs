using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace MetaFoo.Core.IL
{
    public static class PropertyEmitterExtensions
    {
        public static void AddProperty(this TypeDef typeDef, string propertyName, TypeSig propertyType)
        {
            var fieldName = $"__{propertyName}_backingField";
            var backingField = new FieldDefUser(fieldName, new FieldSig(propertyType), FieldAttributes.Private);

            typeDef.Fields.Add(backingField);

            var getterName = $"get_{propertyName}";
            var setterName = $"set_{propertyName}";

            const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig |
                                                MethodAttributes.SpecialName | MethodAttributes.NewSlot |
                                                MethodAttributes.Virtual;

            var module = typeDef.Module;

            // Implement the getter and the setter
            var getter = AddPropertyGetter(propertyType, getterName, attributes, backingField);
            var setter = AddPropertySetter(propertyType, setterName, attributes, backingField, module);

            typeDef.AddProperty(propertyName, propertyType, getter, setter);
        }

        public static void AddProperty(this TypeDef typeDef, string propertyName, TypeSig propertyType,
            MethodDef getter, MethodDef setter)
        {
            var newProperty = new PropertyDefUser(propertyName, new PropertySig(true, propertyType))
            {
                GetMethod = getter,
                SetMethod = setter
            };

            typeDef.Methods.Add(getter);
            typeDef.Methods.Add(setter);
            typeDef.Properties.Add(newProperty);
        }

        private static MethodDefUser AddPropertyGetter(TypeSig propertyType,
            string getterName, MethodAttributes attributes,
            IField backingField)
        {
            var getter = new MethodDefUser(getterName, new MethodSig(CallingConvention.HasThis, 0, propertyType),
                MethodImplAttributes.Managed | MethodImplAttributes.IL, attributes);

            var body = new CilBody();
            body.Instructions.Emit(OpCodes.Ldarg_0);
            body.Instructions.Emit(OpCodes.Ldfld, backingField);
            body.Instructions.Emit(OpCodes.Ret);

            getter.Body = body;

            return getter;
        }

        private static MethodDefUser AddPropertySetter(TypeSig propertyType,
            string setterName, MethodAttributes attributes,
            IField backingField, ModuleDef module)
        {
            var methodSig = new MethodSig(CallingConvention.HasThis, 0, module.CorLibTypes.Void);
            methodSig.Params.Add(propertyType);

            var setter = new MethodDefUser(setterName,
                methodSig,
                MethodImplAttributes.Managed | MethodImplAttributes.IL, attributes);

            var body = new CilBody();
            body.Instructions.Emit(OpCodes.Ldarg_0);
            body.Instructions.Emit(OpCodes.Ldarg_1);
            body.Instructions.Emit(OpCodes.Stfld, backingField);
            body.Instructions.Emit(OpCodes.Ret);

            setter.Body = body;

            return setter;
        }
    }
}
using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace MetaFoo.Core.IL
{
    public static class MethodBodyExtensions
    {
        public static Local AddLocal<T>(this LocalList locals, ModuleDef module)
        {
            return locals.AddLocal(module, typeof(T));
        }

        public static Local AddLocal(this LocalList locals, ModuleDef module, Type type)
        {
            var typeSig = module.ImportAsTypeSig(type);
            return locals.AddLocal(typeSig);
        }

        public static Local AddLocal(this LocalList locals, TypeSig typeSig)
        {
            return locals.Add(new Local(typeSig));
        }
    }
}
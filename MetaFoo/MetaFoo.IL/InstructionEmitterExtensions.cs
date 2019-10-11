using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace MetaFoo.IL
{
    public static class InstructionEmitterExtensions
    {
        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, byte value)
        {
            var instruction = Instruction.Create(opCode, value);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, sbyte value)
        {
            var instruction = Instruction.Create(opCode, value);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, int value)
        {
            var instruction = Instruction.Create(opCode, value);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, long value)
        {
            var instruction = Instruction.Create(opCode, value);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, float value)
        {
            var instruction = Instruction.Create(opCode, value);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, double value)
        {
            var instruction = Instruction.Create(opCode, value);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, string value)
        {
            var instruction = Instruction.Create(opCode, value);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, Instruction target)
        {
            var instruction = Instruction.Create(opCode, target);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, IList<Instruction> targets)
        {
            var instruction = Instruction.Create(opCode, targets);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, ITypeDefOrRef type)
        {
            var instruction = Instruction.Create(opCode, type);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, CorLibTypeSig type)
        {
            var instruction = Instruction.Create(opCode, type);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, MemberRef memberRef)
        {
            var instruction = Instruction.Create(opCode, memberRef);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, IField field)
        {
            var instruction = Instruction.Create(opCode, field);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, IMethod method)
        {
            var instruction = Instruction.Create(opCode, method);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, ITokenOperand token)
        {
            var instruction = Instruction.Create(opCode, token);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, MethodSig methodSig)
        {
            var instruction = Instruction.Create(opCode, methodSig);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, Parameter parameter)
        {
            var instruction = Instruction.Create(opCode, parameter);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, Local local)
        {
            var instruction = Instruction.Create(opCode, local);
            instructions.Add(instruction);
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode)
        {
            var instruction = Instruction.Create(opCode);
            instructions.Add(instruction);
        }

        public static void PackageReturnValue(this ICollection<Instruction> instructions, ModuleDef module,
            ITypeDefOrRef returnType)
        {
            if (returnType.FullName == module.CorLibTypes.Void.FullName)
            {
                instructions.Emit(OpCodes.Pop);
                return;
            }

            instructions.Emit(OpCodes.Unbox_Any, returnType);
        }

        public static void PushMethod(this ICollection<Instruction> instructions, IMethod method, ModuleDef module)
        {
            var getMethodFromHandle = typeof(MethodBase).GetMethods(BindingFlags.Public | BindingFlags.Static).First(
                m =>
                {
                    var parameters = m.GetParameters();
                    if (parameters.Length != 2)
                        return false;

                    var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

                    return parameterTypes[0] == typeof(RuntimeMethodHandle) &&
                           parameterTypes[1] == typeof(RuntimeTypeHandle);
                });

            var importedGetMethodFromHandleMethod = module.Import(getMethodFromHandle);
            var declaringType = method.DeclaringType;
            instructions.Emit(OpCodes.Ldtoken, method);
            instructions.Emit(OpCodes.Ldtoken, declaringType);
            instructions.Emit(OpCodes.Call, importedGetMethodFromHandleMethod);
        }
    }
}
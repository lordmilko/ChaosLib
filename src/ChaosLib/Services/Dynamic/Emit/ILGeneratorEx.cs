#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ChaosLib.Dynamic.Emit
{
    /// <summary>
    /// Provides facilities for adding instructions to an underlying <see cref="ILGenerator"/>.
    /// </summary>
    class ILGeneratorEx
    {
        private ILGenerator il;

        public ILGenerator Generator => il;

        public List<string> Instructions { get; } = new List<string>();

        public ILGeneratorEx(ILGenerator il)
        {
            this.il = il;
        }

        public ArgLocalPair[] CreateArgLocalPairs(Arg[] args)
        {
            var pairs = new List<ArgLocalPair>();

            foreach (var arg in args)
            {
                NamedLocal local = null;

                if (arg.IsByRef && arg.UnwrappedType != null)
                    local = CreateLocal(arg.Name + "Local", arg.UnwrappedType);

                pairs.Add(new ArgLocalPair(arg, local));
            }

            return pairs.ToArray();
        }

        public NamedLocal CreateLocal(string name, Type type)
        {
            var builder = il.DeclareLocal(type);
            builder.SetLocalSymInfo(name);
            return new NamedLocal(name, builder);
        }

        public NamedLabel DefineLabel(string name) => new NamedLabel(name, il.DefineLabel());

        public ILGeneratorEx MarkLabel(NamedLabel label)
        {
            Instructions.Add($"{label.Name}:");
            il.MarkLabel(label.Label);
            return this;
        }

        public ILGeneratorEx Try()
        {
            Instructions.Add("try {");
            il.BeginExceptionBlock();
            return this;
        }

        public ILGeneratorEx Finally()
        {
            Instructions.Add("}");
            Instructions.Add("finally {");
            il.BeginFinallyBlock();
            return this;
        }

        public ILGeneratorEx EndExceptionBlock()
        {
            Instructions.Add("}");
            il.EndExceptionBlock();
            return this;
        }

        public ILGeneratorEx Ldarg(Arg arg)
        {
            switch (arg.Index)
            {
                case 0:
                    Instructions.Add($"ldarg.0 //{arg.Name}");
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    Instructions.Add($"ldarg.2 //{arg.Name}");
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    Instructions.Add($"ldarg.2 //{arg.Name}");
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    Instructions.Add($"ldarg.3 //{arg.Name}");
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    Instructions.Add($"ldarg.s {arg.Index} //{arg.Name}");
                    il.Emit(OpCodes.Ldarg_S, (byte) arg.Index);
                    break;
            }

            return this;
        }

        public ILGeneratorEx Ldloc(NamedLocal local)
        {
            switch (local.Index)
            {
                case 0:
                    Instructions.Add($"ldloc.0 //{local.Name}");
                    il.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    Instructions.Add($"ldloc.1 //{local.Name}");
                    il.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    Instructions.Add($"ldloc.2 //{local.Name}");
                    il.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    Instructions.Add($"ldloc.3 //{local.Name}");
                    il.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    Instructions.Add($"ldloc.s {local.Index} //{local.Name}");
                    il.Emit(OpCodes.Ldloc_S, (byte) local.Index);
                    break;
            }

            return this;
        }

        public ILGeneratorEx Ldfld(FieldInfo field)
        {
            Instructions.Add($"ldfld {field.Name}");
            il.Emit(OpCodes.Ldfld, field);
            return this;
        }

        public ILGeneratorEx Ldloca_S(NamedLocal local)
        {
            Instructions.Add($"ldloca.s {local.Index} //{local.Name}");
            il.Emit(OpCodes.Ldloca_S, (byte) local.Index);
            return this;
        }

        public ILGeneratorEx Callvirt(MethodInfo method)
        {
            Instructions.Add($"callvirt {method.Name}");
            il.Emit(OpCodes.Callvirt, method);
            return this;
        }

        public ILGeneratorEx Call(MethodInfo method)
        {
            Instructions.Add($"call {method.Name}");
            il.Emit(OpCodes.Call, method);
            return this;
        }

        public ILGeneratorEx Ret()
        {
            Instructions.Add("ret");
            il.Emit(OpCodes.Ret);
            return this;
        }

        public ILGeneratorEx Brtrue_S(NamedLabel label)
        {
            Instructions.Add($"brtrue.s {label.Name}");
            il.Emit(OpCodes.Brtrue_S, label.Label);
            return this;
        }

        public ILGeneratorEx Brfalse_S(NamedLabel label)
        {
            Instructions.Add($"brfalse.s {label.Name}");
            il.Emit(OpCodes.Brfalse_S, label.Label);
            return this;
        }

        public ILGeneratorEx Ldnull()
        {
            Instructions.Add("ldnull");
            il.Emit(OpCodes.Ldnull);
            return this;
        }

        public ILGeneratorEx Br_S(NamedLabel label)
        {
            Instructions.Add($"br.s {label.Name}");
            il.Emit(OpCodes.Br_S, label.Label);
            return this;
        }

        public ILGeneratorEx Castclass(Type type)
        {
            Instructions.Add($"castclass {type}");
            il.Emit(OpCodes.Castclass, type);
            return this;
        }

        public ILGeneratorEx Stind_Ref()
        {
            Instructions.Add("stind.ref");
            il.Emit(OpCodes.Stind_Ref);
            return this;
        }

        public ILGeneratorEx Ldind_Ref()
        {
            Instructions.Add("ldind.ref");
            il.Emit(OpCodes.Ldind_Ref);
            return this;
        }

        public ILGeneratorEx Stloc(NamedLocal local)
        {
            switch (local.Index)
            {
                case 0:
                    Instructions.Add($"stloc.0 //{local.Name}");
                    il.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    Instructions.Add($"stloc.1 //{local.Name}");
                    il.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    Instructions.Add($"stloc.2 //{local.Name}");
                    il.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    Instructions.Add($"stloc.3 //{local.Name}");
                    il.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    Instructions.Add($"stloc.s {local.Index} //{local.Name}");
                    il.Emit(OpCodes.Stloc_S, (byte) local.Index);
                    break;
            }

            return this;
        }

        public ILGeneratorEx Newobj(ConstructorInfo ctor)
        {
            Instructions.Add($"newobj {ctor}");
            il.Emit(OpCodes.Newobj, ctor);
            return this;
        }

        public ILGeneratorEx Isinst(Type type)
        {
            Instructions.Add($"Isinst {type}");
            il.Emit(OpCodes.Isinst, type);
            return this;
        }
    }
}
#endif
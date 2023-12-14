#if NETFRAMEWORK
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ChaosLib.Dynamic
{
    class DuckConstructorBuilder : DuckMemberBuilder<ConstructorInfo>
    {
        public static void Build(DuckTypeBuilder typeBuilder)
        {
            var builder = new DuckConstructorBuilder(typeBuilder);

            builder.BuildInternal();
        }

        private DuckConstructorBuilder(DuckTypeBuilder typeBuilder) : base(typeBuilder)
        {
        }

        private void BuildInternal()
        {
            var attribs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var ctor = RawBuilder.DefineConstructor(attribs, CallingConventions.Standard, new[] { typeof(object) });

            var il = ctor.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Castclass, TypeBuilder.InnerType);
            il.Emit(OpCodes.Stfld, TypeBuilder.InnerValue);
            il.Emit(OpCodes.Ret);
        }

        protected override Type GetMemberType(ConstructorInfo member)
        {
            throw new NotSupportedException();
        }
    }
}
#endif
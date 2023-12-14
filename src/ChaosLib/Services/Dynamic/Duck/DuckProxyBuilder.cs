#if NETFRAMEWORK
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ChaosLib.Dynamic
{
    class DuckProxyBuilder
    {
        protected DuckTypeBuilder TypeBuilder { get; }

        protected TypeBuilder RawBuilder => TypeBuilder.Builder;

        protected Type interfaceType;

        public DuckProxyBuilder(DuckTypeBuilder typeBuilder, Type interfaceType)
        {
            TypeBuilder = typeBuilder;
            this.interfaceType = interfaceType;
        }

        public void Build()
        {
            //Private makes it explicit, Public would make it normal
            var attribs = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;

            var interfaceMethod = interfaceType.GetMethod("GetInner");

            var methodBuilder = RawBuilder.DefineMethod("GetInner", attribs, interfaceMethod.ReturnType, null);

            var il = methodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, TypeBuilder.InnerValue);
            il.Emit(OpCodes.Ret);

            RawBuilder.DefineMethodOverride(methodBuilder, interfaceMethod);

            TypeBuilder.GetInnerT = methodBuilder;
        }

        public virtual DuckProxyBuilder GetChildHelper(Type iface) => null;
    }
}
#endif
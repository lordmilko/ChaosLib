#if NETFRAMEWORK
using System;
using System.Reflection;
using System.Reflection.Emit;
using ChaosLib.Dynamic.Emit;

namespace ChaosLib.Dynamic
{
    class DuckPropertyBuilder : DuckMemberBuilder<PropertyInfo>
    {
        public static void Build(DuckInterfaceBuilder interfaceBuilder, PropertyInfo outerInterfaceProperty)
        {
            var builder = new DuckPropertyBuilder(interfaceBuilder, outerInterfaceProperty);

            builder.BuildInternal();
        }

        private PropertyBuilder Builder { get; set; }

        private DuckPropertyBuilder(DuckInterfaceBuilder interfaceBuilder, PropertyInfo outerInterfaceProperty) : base(interfaceBuilder, outerInterfaceProperty)
        {
        }

        private void BuildInternal()
        {
            var innerType = GetParentInnerType();

            InnerMember = innerType.GetProperty(OuterMember.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (InnerMember == null)
                throw new InvalidOperationException($"Could not find a property '{OuterMember.Name}' on inner value of type '{TypeBuilder.InnerType.Name}'.");

            Builder = RawBuilder.DefineProperty(OuterMember.Name, PropertyAttributes.None, OuterMember.PropertyType, null);

            bool isEnumerable;

            if (IsIllegalReturnType(out isEnumerable))
                throw new InvalidOperationException($"Expected property '{OuterMember.Name}' to be a type named '{InnerMember.PropertyType.Name}'. Actual type: '{OuterMember.PropertyType.Name}'.");

            IsEnumerable = isEnumerable;

            BuildGetter();
            BuildSetter();
        }

        protected override Type GetMemberType(PropertyInfo member) => member.PropertyType;

        private void BuildGetter()
        {
            var outerGetter = OuterMember.GetGetMethod();

            if (outerGetter != null)
            {
                var innerGetter = InnerMember.GetGetMethod();

                if (innerGetter == null)
                    throw new InvalidOperationException($"Outer property '{OuterMember.Name}' on interface type '{TypeBuilder.OuterType.Name}' should not have a getter.");

                var methodFlags = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName;

                var methodBuilder = RawBuilder.DefineMethod(outerGetter.Name, methodFlags, OuterMember.PropertyType, null);
                var il = new ILGeneratorEx(methodBuilder.GetILGenerator());

                il
                    .Ldarg(Arg.This)
                    .Ldfld(TypeBuilder.InnerValue)
                    .Callvirt(innerGetter);

                TryWrapMethodResult(il, outerGetter.ReturnType, innerGetter);

                il.Ret();

                Builder.SetGetMethod(methodBuilder);

                RawBuilder.DefineMethodOverride(methodBuilder, outerGetter);
            }
        }

        private void BuildSetter()
        {
            var outerSetter = OuterMember.GetSetMethod();

            if (outerSetter != null)
            {
                var innerSetter = InnerMember.GetSetMethod();

                if (innerSetter == null)
                    throw new InvalidOperationException($"Outer property '{OuterMember.Name}' on interface type '{TypeBuilder.OuterType.Name}' should not have a setter.");

                if (InnerType.Name != OuterType.Name)
                    throw new InvalidOperationException($"Expected property '{OuterMember.Name}' to be of a type type named '{InnerType.Name}'. Actual type: '{OuterType.Name}'.");

                var methodFlags = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName;

                var methodBuilder = RawBuilder.DefineMethod(innerSetter.Name, methodFlags, null, new[] { OuterType });
                var il = new ILGeneratorEx(methodBuilder.GetILGenerator());

                il
                    .Ldarg(Arg.This)
                    .Ldfld(TypeBuilder.InnerValue)
                    .Ldarg(Arg.Value);

                TryGetInner(il, OuterType, InnerType, Arg.Value);

                il
                    .Callvirt(innerSetter)
                    .Ret();

                Builder.SetSetMethod(methodBuilder);

                RawBuilder.DefineMethodOverride(methodBuilder, outerSetter);
            }
        }
    }
}
#endif
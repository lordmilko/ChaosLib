#if NETFRAMEWORK
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ChaosLib.Dynamic.Emit;

namespace ChaosLib.Dynamic
{
    class DuckMethodBuilder : DuckMemberBuilder<MethodInfo>
    {
        #region Static

        public static void Build(DuckInterfaceBuilder interfaceBuilder, MethodInfo outerInterfaceMethod)
        {
            if (IsEventOrProperty(outerInterfaceMethod))
                return;

            var builder = new DuckMethodBuilder(interfaceBuilder, outerInterfaceMethod);

            builder.BuildInternal();
        }

        private static bool IsEventOrProperty(MethodInfo method)
        {
            var ignorePrefix = new[]
            {
                "get_",
                "set_",
                "add_",
                "remove_"
            };

            if (ignorePrefix.Any(i => method.Name.StartsWith(i)))
                return true;

            return false;
        }

        #endregion

        ParameterInfo[] OuterParameters { get; set; }

        ParameterInfo[] InnerParameters { get; set; }

        private DuckMethodBuilder(DuckInterfaceBuilder interfaceBuilder, MethodInfo outerInterfaceMethod) : base(interfaceBuilder, outerInterfaceMethod)
        {
        }

        private void BuildInternal()
        {
            InnerMember = GetMatchingMethod();

            ValidateInnerMethodAndGetParameters();

            var il = new ILGeneratorEx(GetILGenerator());

            var args = OuterParameters.Select((v, i) => new Arg(v, i + 1, InnerParameters[i].ParameterType)).ToArray();

            var pairs = il.CreateArgLocalPairs(args);

            foreach (var pair in pairs)
            {
                if (pair.Local != null && pair.Arg.IsRef)
                    CallGetInnerIfNotNull(il, pair);
            }

            //this.inner
            il
                .Ldarg(Arg.This)
                .Ldfld(TypeBuilder.InnerValue);

            foreach (var pair in pairs)
            {
                if (pair.Local != null)
                {
                    il.Ldloca_S(pair.Local);
                }
                else
                {
                    il.Ldarg(pair.Arg);

                    if (pair.Arg.UnwrappedType != null)
                        CallGetInnerIfNotNull(il, pair.Arg);
                }
            }

            il.Callvirt(InnerMember);

            foreach (var pair in pairs)
            {
                if (pair.Local != null)
                {
                    il
                        .Ldarg(pair.Arg)
                        .Ldloc(pair.Local)
                        .Call(typeof(DynamicExtensions).GetMethod("As").MakeGenericMethod(pair.Arg.Type.GetElementType()))
                        .Stind_Ref();
                }
            }

            TryWrapMethodResult(il, OuterMember.ReturnType, InnerMember);

            il.Ret();
        }

        protected override Type GetMemberType(MethodInfo member) => member.ReturnType;

        private void ValidateInnerMethodAndGetParameters()
        {
            bool isEnumerableReturnType;

            if (IsIllegalReturnType(out isEnumerableReturnType))
                throw new InvalidOperationException($"Expected method '{OuterMember.Name}' to return a value of a type named '{InnerType.Name}'. Actual type: '{OuterType.Name}'.");

            IsEnumerable = isEnumerableReturnType;

            OuterParameters = OuterMember.GetParameters();
            InnerParameters = InnerMember.GetParameters();

            if (OuterParameters.Length != InnerParameters.Length)
                throw new InvalidOperationException($"Expected method '{OuterMember.Name}' on interface type '{TypeBuilder.OuterType.Name}' to have '{InnerParameters.Length}' parameters, however '{OuterParameters.Length}' were found.");

            for (var i = 0; i < OuterParameters.Length; i++)
            {
                if (!IsParameterTypeEqual(OuterParameters[i].ParameterType, InnerParameters[i].ParameterType))
                    throw new InvalidOperationException($"Expected parameter {i} on interface '{TypeBuilder.OuterType.Name}' method '{OuterMember.Name}' to be of type '{InnerParameters[i].Name}'. Actual type: '{OuterParameters[i].Name}'.");
            }
        }

        protected virtual ILGenerator GetILGenerator()
        {
            var methodFlags = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;

            var methodBuilder = RawBuilder.DefineMethod(
                OuterMember.Name,
                methodFlags,
                OuterType,
                OuterParameters.Select(p => p.ParameterType).ToArray()
            );

            var il = methodBuilder.GetILGenerator();

            return il;
        }

        private MethodInfo GetMatchingMethod()
        {
            MethodInfo innerMethod;

            var innerType = GetParentInnerType();

            //We aren't getting the method by its parameter types because our proxy method may contain proxy types in its parameters
            var innerMethods = innerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m => m.Name == OuterMember.Name).ToArray();

            if (innerMethods.Length == 1)
                innerMethod = innerMethods[0];
            else
            {
                var length = OuterMember.GetParameters().Length;
                var lengthMatches = innerMethods.Where(m => m.GetParameters().Length == length).ToArray();

                if (lengthMatches.Length == 1)
                    innerMethod = lengthMatches[0];
                else
                    throw new InvalidOperationException($"Cannot find a matching member on type '{TypeBuilder.InnerType.Name}' for method '{OuterMember.Name}'.");
            }

            if (innerMethod == null)
                throw new InvalidOperationException($"Could not find a method '{OuterMember.Name}' on inner value of type '{InnerType.Name}'.");

            return innerMethod;
        }
    }
}
#endif
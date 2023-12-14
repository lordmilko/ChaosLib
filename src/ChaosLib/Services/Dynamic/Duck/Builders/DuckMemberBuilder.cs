#if NETFRAMEWORK
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ChaosLib.Dynamic.Emit;

namespace ChaosLib.Dynamic
{
    abstract class DuckMemberBuilder<TMember> where TMember : MemberInfo
    {
        public DuckInterfaceBuilder InterfaceBuilder { get; }

        public DuckTypeBuilder TypeBuilder { get; }

        protected TypeBuilder RawBuilder => TypeBuilder.Builder;

        public TMember OuterMember { get; }

        public TMember InnerMember { get; protected set; }

        /// <summary>
        /// Gets the outer type of the member, i.e. the <see cref="MethodInfo.ReturnType"/>, <see cref="PropertyInfo.PropertyType"/>, etc.
        /// </summary>
        protected Type OuterType => GetMemberType(OuterMember);

        /// <summary>
        /// Gets the inner type of the member, i.e. the <see cref="MethodInfo.ReturnType"/>, <see cref="PropertyInfo.PropertyType"/>, etc.
        /// </summary>
        protected Type InnerType => GetMemberType(InnerMember);

        protected bool IsEnumerable { get; set; }

        protected DuckMemberBuilder(DuckInterfaceBuilder interfaceBuilder, TMember outerMember)
        {
            InterfaceBuilder = interfaceBuilder;
            TypeBuilder = InterfaceBuilder.TypeBuilder;
            OuterMember = outerMember;
        }

        protected DuckMemberBuilder(DuckTypeBuilder typeBuilder)
        {
            TypeBuilder = typeBuilder;
        }

        protected abstract Type GetMemberType(TMember member);

        protected bool IsIllegalReturnType(out bool isEnumerableReturnType)
        {
            if (OuterType == typeof(object))
            {
                isEnumerableReturnType = false;

                if (InnerType.IsValueType)
                    return true;

                return false;
            }

            if (typeof(IEnumerable).IsAssignableFrom(OuterType) && OuterType != typeof(string))
            {
                isEnumerableReturnType = true;
                return false;
            }

            if (IsEquivalentEnum())
            {
                isEnumerableReturnType = false;
                return false;
            }

            isEnumerableReturnType = false;
            return InnerType.Name != OuterType.Name;
        }

        private bool IsEquivalentEnum()
        {
            if (!InnerType.IsEnum || !OuterType.IsEnum)
                return false;

            var outerValues = Enum.GetNames(OuterType);
            var innerValues = Enum.GetNames(InnerType);

            //If there are any values in our type that do not exist in the inner type,
            //then even if the names of the enums might not necessarily be the same (because we renamed them)
            //we can at least say the two enums are equivalent enough
            if (outerValues.Except(innerValues).Any())
                return false;

            return true;
        }

        protected bool IsParameterTypeEqual(Type outerType, Type innerType)
        {
            if (innerType.IsByRef && !outerType.IsByRef || !innerType.IsByRef && outerType.IsByRef)
                return false;

            if (outerType.Name == innerType.Name)
                return true;

            Type outerProxyType;

            if (TryGetOrBuildProxyType(outerType, innerType, out outerProxyType))
            {
                if (outerProxyType.GetGenericArguments()[0].Name == innerType.Name)
                    return true;
            }

            return false;
        }

        protected void TryGetInner(ILGeneratorEx il, Type outerType, Type innerType, Arg arg)
        {
            Type valueProxyType;

            if (TryGetOrBuildProxyType(outerType, innerType, out valueProxyType))
            {
                var notNull = il.DefineLabel("notNull");
                var afterNotNull = il.DefineLabel("afterNotNull");

                il
                    .Brtrue_S(notNull)
                    .Ldnull()
                    .Br_S(afterNotNull)

                    .MarkLabel(notNull)
                    .Ldarg(arg)
                    .Castclass(valueProxyType)
                    .Callvirt(valueProxyType.GetMethod("GetInner"))

                    .MarkLabel(afterNotNull);
            }
        }

        protected void CallGetInnerIfNotNull(ILGeneratorEx il, Arg arg)
        {
            Debug.Assert(arg.UnwrappedType != null);

            var notNull = il.DefineLabel("notNull");
            var afterNotNull = il.DefineLabel("afterNotNull");

            il
                .Brtrue_S(notNull) //The target is the argument we already loaded prior to calling CallGetInnerIfNotNull
                .Ldnull()
                .Br_S(afterNotNull)

                .MarkLabel(notNull)
                .Ldarg(arg)
                .Castclass(arg.IProxyT)
                .Callvirt(arg.IProxyT.GetMethod("GetInner"))

                .MarkLabel(afterNotNull);
        }

        protected void CallGetInnerIfNotNull(ILGeneratorEx il, ArgLocalPair pair)
        {
            Debug.Assert(pair.Arg.UnwrappedType != null);

            var notNull = il.DefineLabel("notNull");
            var afterNotNull = il.DefineLabel("afterNotNull");

            il
                .Ldarg(pair.Arg)
                .Ldind_Ref()
                .Brtrue_S(notNull)
                .Ldnull()
                .Br_S(afterNotNull)

                .MarkLabel(notNull)
                .Ldarg(pair.Arg)
                .Ldind_Ref()
                .Castclass(pair.Arg.IProxyT)
                .Callvirt(pair.Arg.IProxyT.GetMethod("GetInner"))

                .MarkLabel(afterNotNull)
                .Stloc(pair.Local);
        }

        private static bool ShouldCreateProxy(Type outerType, Type innerType)
        {
            var attribs = outerType.GetCustomAttribute<ReflectedTypeAttribute>();

            if (attribs != null)
                return true;

            if (!outerType.IsInterface)
                return false;

            if (outerType.IsAssignableFrom(innerType))
                return false;

            if (outerType.Name == innerType.Name)
                return true;

            return false;
        }

        public static bool TryGetOrBuildProxyType(Type outerType, Type innerType, out Type proxyInterfaceType)
        {
            if (DuckTypeManager.TryGetIProxyTFromInterfaceType(outerType, out proxyInterfaceType))
                return true;

            if (ShouldCreateProxy(outerType, innerType))
            {
                var proxyType = DuckTypeManager.Build(outerType, innerType);

                var iface = proxyType.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDuckProxy<>));

                proxyInterfaceType = iface;
                return true;
            }

            proxyInterfaceType = null;
            return false;
        }

        protected void TryWrapMethodResult(ILGeneratorEx il, Type outerReturnType, MethodInfo innerMethod)
        {
            if (IsEnumerable)
            {
                Type elementType = null;

                if (outerReturnType.IsArray)
                    elementType = outerReturnType.GetElementType();
                else
                {
                    if (outerReturnType.IsGenericType && outerReturnType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        elementType = outerReturnType.GetGenericArguments()[0];
                    else
                    {
                        foreach (var iface in outerReturnType.GetInterfaces())
                        {
                            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            {
                                elementType = iface.GetGenericArguments()[0];

                                break;
                            }
                        }
                    }
                }

                var attrib = elementType?.GetCustomAttribute<ReflectedTypeAttribute>();

                if (attrib != null || outerReturnType != innerMethod.ReturnType)
                    GenerateArrayHelper(il, innerMethod, elementType);
            }
            else
            {
                if (ShouldCreateProxy(outerReturnType, innerMethod.ReturnType))
                {
                    var asMethod = typeof(DynamicExtensions).GetMethod("As").MakeGenericMethod(outerReturnType);

                    il.Call(asMethod);
                }
            }
        }

        private void GenerateArrayHelper(ILGeneratorEx il, MethodInfo innerMethod, Type outerElementType)
        {
            var listType = typeof(List<>).MakeGenericType(outerElementType);

            var getEnumerator = GetGetEnumeratorMethod(innerMethod.ReturnType);
            var asMethod = typeof(DynamicExtensions).GetMethod("As").MakeGenericMethod(outerElementType);

            var originalResult = il.CreateLocal("originalResult", innerMethod.ReturnType);
            var newList = il.CreateLocal("newList", listType);
            var enumerator = il.CreateLocal("enumerator", typeof(IEnumerator));
            var current = il.CreateLocal("current", typeof(object));
            var disposable = il.CreateLocal("disposable", typeof(IDisposable));

            var ctor = listType.GetConstructor(new Type[0]);

            if (ctor == null)
                throw new InvalidOperationException($"Could not find the constructor to use for type '{listType.Name}'.");

            il
                .Stloc(originalResult) //we call the inner method/property prior to calling GenerateArrayHelper

                .Newobj(ctor)
                .Stloc(newList)

                .Ldloc(originalResult)
                .Callvirt(getEnumerator)
                .Stloc(enumerator);

            var preMoveNext = il.DefineLabel("preMoveNext");
            var loopStart = il.DefineLabel("loopStart");
            var end = il.DefineLabel("end");

            il
                .Try()
                    .Br_S(preMoveNext) //jump to MoveNext
                    .MarkLabel(loopStart)

                    .Ldloc(enumerator)
                    .Callvirt(typeof(IEnumerator).GetProperty("Current").GetGetMethod())
                    .Stloc(current)

                    .Ldloc(newList)
                    .Ldloc(current)
                    .Call(asMethod)
                    .Callvirt(listType.GetMethod("Add"))

                    .MarkLabel(preMoveNext)
                    .Ldloc(enumerator)
                    .Callvirt(typeof(IEnumerator).GetMethod("MoveNext"))
                    .Brtrue_S(loopStart) //jump to loopStart if MoveNext returned true

                .Finally()
                    .Ldloc(enumerator)
                    .Isinst(typeof(IDisposable))
                    .Stloc(disposable)

                    .Ldloc(disposable)
                    .Brfalse_S(end)

                    .Ldloc(disposable)
                    .Callvirt(typeof(IDisposable).GetMethod("Dispose"))

                    .MarkLabel(end)

                .EndExceptionBlock();

            //list.ToArray()
            il
                .Ldloc(newList)
                .Callvirt(listType.GetMethod("ToArray"))
                .Ret();
        }

        private MethodInfo GetGetEnumeratorMethod(Type type)
        {
            if (type.IsInterface)
                return typeof(IEnumerable).GetMethod("GetEnumerator");

            return type.GetMethod("GetEnumerator");
        }

        protected Type GetParentInnerType()
        {
            var innerType = TypeBuilder.InnerType;

            var attribs = (ReflectedTypeAttribute)OuterMember.DeclaringType.GetCustomAttributes().FirstOrDefault(t => t.GetType() == typeof(ReflectedTypeAttribute));

            if (attribs != null)
                return attribs.Type;

            if (OuterMember.DeclaringType.Name == innerType.Name)
                return innerType;

            var interfaces = innerType.GetInterfaces();

            foreach (var iface in interfaces)
            {
                if (OuterMember.DeclaringType.Name == iface.Name)
                    return iface;
            }

            return innerType;
        }
    }
}
#endif
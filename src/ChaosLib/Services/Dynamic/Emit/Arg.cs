#if NETFRAMEWORK
using System;
using System.Diagnostics;
using System.Reflection;

namespace ChaosLib.Dynamic.Emit
{
    /// <summary>
    /// Represents an argument to a method that should be specified in conjunction
    /// with a Ldarg opcode.
    /// </summary>
    [DebuggerDisplay("{Type.Namespace,nq}.{Type.Name,nq} {Name,nq} (Index: {Index}, Proxy: {UnwrappedType != null})")]
    class Arg
    {
        public static readonly Arg This = new Arg("this", 0);

        public static readonly Arg Value = new Arg("value", 1);

        public string Name { get; }

        public int Index { get; }

        public bool IsByRef { get; }

        public bool IsRef { get; }

        public bool IsOut { get; }

        public Type Type { get; }

        public Type IProxyT { get; }

        public Type UnwrappedType { get; }

        public Arg(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public Arg(ParameterInfo outerParameter, int index, Type innerType)
        {
            Name = outerParameter.Name;
            Index = index;
            IsByRef = outerParameter.ParameterType.IsByRef;
            IsRef = IsByRef && !outerParameter.IsOut;
            IsOut = IsByRef && outerParameter.IsOut;
            Type = outerParameter.ParameterType;

            Type proxyTType;

            var outerType = outerParameter.ParameterType;

            if (IsByRef)
            {
                outerType = outerType.GetElementType();
                innerType = innerType.GetElementType();
            }

            if (DuckMemberBuilder<MemberInfo>.TryGetOrBuildProxyType(outerType, innerType, out proxyTType))
            {
                IProxyT = proxyTType;
                UnwrappedType = proxyTType.GetGenericArguments()[0];
            }
        }
    }
}
#endif
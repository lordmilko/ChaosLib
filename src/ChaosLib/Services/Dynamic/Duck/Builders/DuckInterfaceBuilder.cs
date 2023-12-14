#if NETFRAMEWORK
using System;
using System.Reflection;

namespace ChaosLib.Dynamic
{
    class DuckInterfaceBuilder
    {
        public static void Build(DuckTypeBuilder typeBuilder, Type outerInterfaceType, DuckProxyBuilder proxyBuilder = null)
        {
            if (typeBuilder.Interfaces.Contains(outerInterfaceType))
                return;

            typeBuilder.AddInterface(outerInterfaceType);

            if (proxyBuilder != null)
                proxyBuilder.Build();
            else
            {
                var builder = new DuckInterfaceBuilder(typeBuilder, outerInterfaceType);
                builder.BuildInternal();
            }

            var parentInterfaces = outerInterfaceType.GetInterfaces();

            foreach (var iface in parentInterfaces)
                Build(typeBuilder, iface, proxyBuilder?.GetChildHelper(iface));
        }

        public DuckTypeBuilder TypeBuilder { get; }

        Type OuterInterfaceType { get; }

        private DuckInterfaceBuilder(DuckTypeBuilder typeBuilder, Type outerInterfaceType)
        {
            TypeBuilder = typeBuilder;
            OuterInterfaceType = outerInterfaceType;
        }

        private void BuildInternal()
        {
            var members = OuterInterfaceType.GetMembers();

            foreach (var member in members)
            {
                if (member is PropertyInfo)
                    DuckPropertyBuilder.Build(this, (PropertyInfo) member);
                else if (member is MethodInfo)
                    DuckMethodBuilder.Build(this, (MethodInfo) member);
                else
                    throw new NotImplementedException($"Don't know how to handle member of type '{member.Name}'.");
            }
        }
    }
}
#endif
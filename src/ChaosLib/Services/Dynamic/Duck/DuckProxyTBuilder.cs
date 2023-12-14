#if NETFRAMEWORK
using System;

namespace ChaosLib.Dynamic
{
    class DuckProxyTBuilder : DuckProxyBuilder
    {
        public DuckProxyTBuilder(DuckTypeBuilder typeBuilder, Type interfaceType) : base(typeBuilder, interfaceType)
        {
        }

        public override DuckProxyBuilder GetChildHelper(Type iface)
        {
            if (iface == typeof(IDuckProxy))
                return new DuckProxyBuilder(TypeBuilder, iface);

            return null;
        }
    }
}
#endif
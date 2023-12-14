#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ChaosLib.Dynamic
{
    class DuckTypeBuilder
    {
        /// <summary>
        /// Gets the outer type this proxy is being presented as e.g. ChaosLib.IFoo.<para/>
        /// This type is always an interface.
        /// </summary>
        public Type OuterType { get; }

        /// <summary>
        /// Gets the type that is stored in the proxy's inner field e.g. Microsoft.VisualStudio.Foo / Microsoft.VisualStudio.IFoo.<para/>
        /// This type may or may not be an interface.
        /// </summary>
        public Type InnerType { get; }

        public Type IProxyT { get; }

        public List<Type> Interfaces { get; } = new List<Type>();

        #region IProxy

        public FieldInfo InnerValue { get; }

        public MethodInfo GetInnerT { get; set; }

        #endregion

        public TypeBuilder Builder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuckTypeBuilder"/> class.
        /// </summary>
        /// <param name="outerType">The outer interface type that should be used for the duck typing.</param>
        /// <param name="innerType">The inner type that should be wrapped in the duck type. This type may or may not be an interface.</param>
        public DuckTypeBuilder(Type outerType, Type innerType)
        {
            OuterType = outerType;
            InnerType = innerType;
            IProxyT = typeof(IDuckProxy<>).MakeGenericType(innerType);

            DuckTypeManager.RegisterProxy(outerType, IProxyT);

            Builder = DynamicAssembly.Instance.DefineProxy(innerType.FullName);

            InnerValue = DuckFieldBuilder.Build(this, innerType, "inner");

            DuckConstructorBuilder.Build(this);
            DuckInterfaceBuilder.Build(this, IProxyT, new DuckProxyTBuilder(this, IProxyT));

            DuckInterfaceBuilder.Build(this, OuterType);
        }

        public void AddInterface(Type iface)
        {
            Builder.AddInterfaceImplementation(iface);
            Interfaces.Add(iface);
        }
    }
}
#endif
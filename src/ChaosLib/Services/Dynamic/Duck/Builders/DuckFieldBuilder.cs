#if NETFRAMEWORK
using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace ChaosLib.Dynamic
{
    class DuckFieldBuilder : DuckMemberBuilder<FieldInfo>
    {
        public static FieldInfo Build(DuckTypeBuilder typeBuilder, Type fieldType, string name)
        {
            var builder = new DuckFieldBuilder(typeBuilder, fieldType, name);

            builder.BuildInternal();

            return builder.Builder;
        }

        private FieldBuilder Builder { get; set; }

        private string Name { get; }

        private DuckFieldBuilder(DuckTypeBuilder typeBuilder, Type fieldType, string name) : base(typeBuilder)
        {
            Name = name;
        }

        private void BuildInternal()
        {
            var attributes = FieldAttributes.Private;

            Builder = RawBuilder.DefineField(Name, TypeBuilder.InnerType, attributes);

            var attribute = typeof(DebuggerBrowsableAttribute);
            var ctor = attribute.GetConstructor(new[] { typeof(DebuggerBrowsableState) });

            var attributeBuilder = new CustomAttributeBuilder(ctor, new object[] { DebuggerBrowsableState.Never });

            Builder.SetCustomAttribute(attributeBuilder);
        }

        protected override Type GetMemberType(FieldInfo member) => member.FieldType;
    }
}
#endif
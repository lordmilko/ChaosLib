using System;
using ClrDebug.DIA;

namespace ChaosLib.TypedData
{
    public class DbgRemoteField
    {
        public long Address { get; }

        public string Name { get; }

        public long Offset { get; }

        public DbgRemoteType Type { get; }

        public IDbgRemoteValue Value { get; }

        public DbgRemoteFieldCollection Fields => (Value as DbgRemoteObject)?.Fields ?? DbgRemoteFieldCollection.Empty;

        public DbgRemoteObject Parent { get; }

        public static DbgRemoteField New(long address, DbgRemoteFieldInfo info, DbgRemoteObject parent, ITypedDataProvider provider)
        {
            if (info.Type.Tag == SymTagEnum.BaseType)
                return new DbgRemoteField(address, info, parent, GetSimpleValue(address, info, provider));
            else
            {
                DbgRemoteObject value;

                if (info.Type.Name == "_LIST_ENTRY" && parent.Type.Name != "_LIST_ENTRY")
                    value = new DbgRemoteListEntryHead(address, info.Type, provider);
                else if (info.Type.Name == "_UNICODE_STRING")
                    value = new DbgRemoteUnicodeString(address, info.Type, provider);
                else
                    value = new DbgRemoteObject(address, info.Type, provider);

                return new DbgRemoteComplexField(address, info, parent, value);
            }
        }

        protected DbgRemoteField(long address, DbgRemoteFieldInfo info, DbgRemoteObject parent, IDbgRemoteValue value)
        {
            Name = info.Name;
            Offset = info.Offset;
            Type = info.Type;

            Address = address;
            Parent = parent;
            Value = value;
        }

        private static IDbgRemoteValue GetSimpleValue(long address, DbgRemoteFieldInfo info, ITypedDataProvider provider)
        {
            var value = provider.ReadVirtual(address, info.Type.Length);

            IDbgRemoteValue a = new DbgRemotePrimitiveValue<bool>(true);

            switch (info.Type.Length)
            {
                case 1:
                    return new DbgRemotePrimitiveValue<bool>(value[0] == 1);
                case 2:
                    return new DbgRemotePrimitiveValue<ushort>(BitConverter.ToUInt16(value, 0));
                case 4:
                    return new DbgRemotePrimitiveValue<uint>(BitConverter.ToUInt32(value, 0));
                case 8:
                    return new DbgRemotePrimitiveValue<ulong>(BitConverter.ToUInt64(value, 0));
                default:
                    throw new NotImplementedException($"Don't know how to handle a simple value of {info.Type.Length} bytes");
            }
        }

        public DbgRemoteField this[string name] => Fields[name];

        public override string ToString()
        {
            if (Value is DbgRemotePrimitiveValue p)
            {
                var v = p.Value;

                if (v is ushort || v is uint || v is ulong)
                    v = $"0x{Convert.ToUInt64(v):X}";

                return $"{Name} : {v}";
            }

            var value = $"0x{Address:X}";

            if (Value is DbgRemoteUnicodeString s)
                value = s.String ?? "null";

            return $"{Type} : {Name} : {value}";
        }
    }
}
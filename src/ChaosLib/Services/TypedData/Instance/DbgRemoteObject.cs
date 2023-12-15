using System.Collections.Generic;
using System.Diagnostics;
using ClrDebug.DIA;

namespace ChaosLib.TypedData
{
    public class DbgRemoteObject : IDbgRemoteValue
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IDbgRemoteValue.Value => this;

        public DbgRemoteType Type { get; }

        /// <summary>
        /// Gets the address at which this struct resides.
        /// </summary>
        public long Address { get; }

        public DbgRemoteFieldCollection Fields
        {
            get
            {
                var results = new List<DbgRemoteField>();

                foreach (var field in Type.Fields)
                {
                    var addr = Address + field.Offset;

                    if (field.Type.Tag == SymTagEnum.PointerType)
                        addr = provider.ReadPointer(addr);

                    results.Add(DbgRemoteField.New(addr, field, this, provider));
                }

                return new DbgRemoteFieldCollection(results.ToArray());
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected ITypedDataProvider provider;

        public DbgRemoteObject(long address, string type, ITypedDataProvider provider)
        {
            Address = address;
            Type = DbgRemoteType.New(type, provider);

            this.provider = provider;
        }

        public DbgRemoteObject(long address, DbgRemoteType type, ITypedDataProvider provider)
        {
            Address = address;
            Type = type;

            this.provider = provider;
        }

        public IDbgRemoteValue this[string name] => Fields[name]?.Value;

        public override string ToString()
        {
            return $"{Type} : 0x{Address:X}";
        }
    }
}
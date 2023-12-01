using System.Diagnostics;

namespace ChaosLib.TypedData
{
    class DbgRemoteComplexFieldDebugView
    {
        private DbgRemoteComplexField field;

        public DbgRemoteComplexFieldDebugView(DbgRemoteComplexField field)
        {
            this.field = field;
        }

        public long Address => field.Address;

        public string Name => field.Name;

        public long Offset => field.Offset;

        public DbgRemoteType Type => field.Type;

        public DbgRemoteObject Value => field.Value;

        public DbgRemoteFieldCollection Fields => field.Fields;
    }

    [DebuggerTypeProxy(typeof(DbgRemoteComplexFieldDebugView))]
    class DbgRemoteComplexField : DbgRemoteField
    {
        public new DbgRemoteObject Value => (DbgRemoteObject) base.Value;

        public DbgRemoteComplexField(long address, DbgRemoteFieldInfo info, DbgRemoteObject parent, DbgRemoteObject value) : base(address, info, parent, value)
        {
        }
    }
}
namespace ChaosLib.TypedData
{
    class DbgRemotePrimitiveValue : IDbgRemoteValue
    {
        public object Value { get; }

        public DbgRemotePrimitiveValue(object value)
        {
            Value = value;
        }

        public IDbgRemoteValue this[string name] => null;
    }

    class DbgRemotePrimitiveValue<T> : DbgRemotePrimitiveValue
    {
        public new T Value => (T)base.Value;

        public DbgRemotePrimitiveValue(T value) : base(value)
        {
        }

        public static implicit operator T(DbgRemotePrimitiveValue<T> value) => value.Value;
    }
}
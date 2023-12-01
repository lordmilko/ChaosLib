namespace ChaosLib.TypedData
{
    public class DbgRemoteFieldInfo
    {
        public string Name { get; }

        public long Offset { get; }

        public DbgRemoteType Type { get; }

        public DbgRemoteFieldInfo(long moduleBase, int typeId, ITypedDataProvider provider)
        {
            var item = provider.GetTypeInfo(moduleBase, typeId);

            Name = item.SymName;
            Offset = item.Offset;

            Type = DbgRemoteType.New(item, provider);
        }

        public override string ToString()
        {
            return $"{Type} : {Name}";
        }
    }
}
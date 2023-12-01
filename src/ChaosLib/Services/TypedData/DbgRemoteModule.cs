namespace ChaosLib.TypedData
{
    public class DbgRemoteModule
    {
        public string Name { get; }

        public long BaseAddress { get; }

        public DbgRemoteModule(string name, long baseAddress)
        {
            Name = name;
            BaseAddress = baseAddress;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
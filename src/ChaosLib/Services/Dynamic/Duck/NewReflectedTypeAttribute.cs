#if NETFRAMEWORK
namespace ChaosLib.Dynamic
{
    public class NewReflectedTypeAttribute : ReflectedTypeAttribute
    {
        public NewReflectedTypeAttribute(string assemblyName, string typeName) : base(assemblyName, typeName)
        {
        }
    }
}
#endif
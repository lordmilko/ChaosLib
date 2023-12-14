#if NETFRAMEWORK
namespace ChaosLib.Dynamic.Emit
{
    /// <summary>
    /// Represents an argument to be specified to a method that should be
    /// specified in conjunction with a Ldarg opcode and and a local for
    /// that argument to be assigned to once converted to another value.
    /// </summary>
    class ArgLocalPair
    {
        public Arg Arg { get; }

        public NamedLocal Local { get; }

        public ArgLocalPair(Arg arg, NamedLocal local)
        {
            Arg = arg;
            Local = local;
        }
    }
}
#endif
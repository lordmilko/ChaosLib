#if NETFRAMEWORK
using System;
using System.Diagnostics;
using System.Reflection.Emit;

namespace ChaosLib.Dynamic
{
    [DebuggerDisplay("{Type.Namespace,nq}.{Type.Name,nq} {Name,nq} (Index: {Index})")]
    class NamedLocal
    {
        private LocalBuilder builder;

        public string Name { get; }

        public int Index => builder.LocalIndex;

        public Type Type => builder.LocalType;

        public NamedLocal(string name, LocalBuilder builder)
        {
            Name = name;
            this.builder = builder;
        }
    }
}
#endif
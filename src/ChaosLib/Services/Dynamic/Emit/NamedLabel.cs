#if NETFRAMEWORK
using System.Reflection.Emit;

namespace ChaosLib.Dynamic.Emit
{
    /// <summary>
    /// Encapsulates a <see cref="Label"/> and its name.
    /// </summary>
    class NamedLabel
    {
        public string Name { get; }

        public Label Label { get; }

        public NamedLabel(string name, Label label)
        {
            Name = name;
            Label = label;
        }
    }
}
#endif
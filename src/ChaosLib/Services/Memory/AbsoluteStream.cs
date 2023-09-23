using System.IO;

namespace ChaosLib.Memory
{
    /// <summary>
    /// Represents a stream whose positions are based on absolute addresses (such as a stream encapsulating
    /// the memory of a target process).
    /// </summary>
    public abstract class AbsoluteStream : Stream
    {
    }
}

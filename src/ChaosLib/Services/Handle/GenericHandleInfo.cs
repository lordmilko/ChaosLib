using System;

namespace ChaosLib.Handle
{
    /// <summary>
    /// Represents a handle for which no more specific <see cref="HandleInfo"/> type is defined.
    /// </summary>
    public class GenericHandleInfo : HandleInfo
    {
        public GenericHandleInfo(IntPtr raw, HandleType type) : base(raw, type)
        {
        }
    }
}
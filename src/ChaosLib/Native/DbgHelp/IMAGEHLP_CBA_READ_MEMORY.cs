using System;

namespace ChaosLib
{
    /// <summary>
    /// Contains information about a memory read operation.
    /// </summary>
    public unsafe struct IMAGEHLP_CBA_READ_MEMORY
    {
        /// <summary>
        /// The address to be read.
        /// </summary>
        public long addr;

        /// <summary>
        /// A pointer to a buffer that receives the memory read.
        /// </summary>
        public IntPtr buf;

        /// <summary>
        /// The number of bytes to read.
        /// </summary>
        public int bytes;

        /// <summary>
        /// A pointer to a variable that receives the number of bytes read.
        /// </summary>
        public int* bytesread;
    }
}
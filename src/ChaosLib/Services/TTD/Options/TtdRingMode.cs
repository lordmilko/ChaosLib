namespace ChaosLib.TTD
{
    /// <summary>
    /// Specify how to record a ring trace.
    /// </summary>
    public enum TtdRingMode
    {
        /// <summary>
        /// The ring will be in a file on disk. This is the default.
        /// </summary>
        file,

        /// <summary>
        /// The ring will be in a file, but the entire file will be fully mapped in memory. This reduces the
        /// I/O overhead, but the entire file is mapped in contiguous address space, which may add significant
        /// memory pressure to 32-bit processes.
        /// </summary>
        mappedFile
    }
}
namespace ChaosLib.TTD
{
    /// <summary>
    /// Specifies what support is expected from the CPUs that will be used to replay the trace.
    /// </summary>
    public enum TTdReplayCpuSupportMode
    {
        /// <summary>
        /// Default CPU support, just requires basic commonly-available support in the replay CPU.
        /// </summary>
        Default,

        /// <summary>
        /// Requires no special support in the replay CPU. Adequate for traces that will be replayed on a
        /// completely different CPU architecture, like an Intel trace on ARM64.
        /// </summary>
        MostConservative,

        /// <summary>
        /// Assumes that the replay CPU will be similar and of equal or greater capability than the CPU used to record.
        /// </summary>
        MostAggressive,

        /// <summary>
        /// Assumes that the replay CPU will be Intel/AMD 64-bit CPU supporting AVX.
        /// </summary>
        IntelAvxRequired,

        /// <summary>
        /// Assumes that the replay CPU will be Intel/AMD 64-bit CPU supporting AVX2.
        /// </summary>
        IntelAvx2Required
    }
}
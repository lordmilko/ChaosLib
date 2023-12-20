namespace ChaosLib.TTD
{
    public enum TTD_TRACE_MODE
    {
        /// <summary>
        /// Indicates that TTD should include all <see cref="Unrestricted"/> options in addition to all Restricted and Unsupported options.<para/>
        /// This is option grants the highest level of access to the capabilities of TTD.
        /// </summary>
        Extended = 0,

        /// <summary>
        /// Indicates that TTD should include additional options not normally externally visible.<para/>
        /// This option grants the second highest level of access to the capabilities of TTD.
        /// </summary>
        Unrestricted = 1,

        /// <summary>
        /// Indicates that TTD should be limited to the default, limited set of options. This is the default in external builds of TTD/TTTracer.<para/>
        /// This option grants the lowest level of access to the capabilities of TTD.
        /// </summary>
        Standalone = 2
    }
}
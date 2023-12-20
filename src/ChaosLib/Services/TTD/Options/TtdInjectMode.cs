namespace ChaosLib.TTD
{
    /// <summary>
    /// Specify how recording DLLs are injected into the target process. If not specified, <see cref="TtdInjectMode.LoaderForCombinedRecording"/> is
    /// assumed by default.
    /// </summary>
    public enum TtdInjectMode
    {
        /// <summary>
        /// Inject the loader to load one combined DLL for recording. This is the default value.
        /// </summary>
        LoaderForCombinedRecording,

        /// <summary>
        /// Inject the loader to load the emulator without recording, useful for measuring emulation behaviors and performance.
        /// </summary>
        LoaderForEmulator,

        /// <summary>
        /// Inject one combined DLL for recording, useful for recording sandbox processes like Chromium.
        /// </summary>
        EmulatorForRecording,

        /// <summary>
        /// Inject only the emulator without recording, useful for measuring emulation behaviors and performance.
        /// </summary>
        EmulatorOnly
    }
}
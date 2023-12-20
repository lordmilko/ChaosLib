using System.ComponentModel;

namespace ChaosLib.Handle
{
    /// <summary>
    /// Specifies types of operating system handles that are known to exist.
    /// </summary>
    public enum HandleType
    {
        /// <summary>
        /// The handle is of an unknown type not listed in <see cref="HandleType"/>.
        /// </summary>
        Unknown = 0,
        
        [Description("ALPC Port")]
        ALPCPort,
        Event,
        DebugObject,
        Desktop,
        Directory,
        EtwRegistration,
        File,
        Key,
        IoCompletion,
        IRTimer,
        Mutant,
        Process,
        Section,
        Semaphore,
        Thread,
        Timer,
        TpWorkerFactory,
        WaitCompletionPacket,
        WindowStation,

        /// <summary>
        /// The type of handle could not be identified as it was reported as being invalid.
        /// </summary>
        Invalid
    }
}
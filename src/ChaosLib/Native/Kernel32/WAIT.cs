namespace ChaosLib
{
    public enum WAIT
    {
        /// <summary>
        /// The specified object is a mutex object that was not released by the thread that owned the mutex object before
        /// the owning thread terminated. Ownership of the mutex object is granted to the calling thread and the mutex state
        /// is set to nonsignaled. the mutex was protecting persistent state information, you should check it for consistency.
        /// </summary>
        ABANDONED = 0x00000080,

        /// <summary>
        /// The state of the specified object is signaled.
        /// </summary>
        OBJECT_0 = 0x00000000,

        /// <summary>
        /// The time-out interval elapsed, and the object's state is nonsignaled.
        /// </summary>
        TIMEOUT = 0x00000102,

        /// <summary>
        /// The function has failed. To get extended error information, call GetLastError.
        /// </summary>
        FAILED = -1
    }
}

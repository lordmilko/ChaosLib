namespace ChaosLib
{
    public enum LDR_DLL_NOTIFICATION_REASON
    {
        /// <summary>
        /// The DLL was loaded. The NotificationData parameter points to an <see cref="LDR_DLL_LOADED_NOTIFICATION_DATA"/> structure.
        /// </summary>
        LOADED = 1,

        /// <summary>
        /// The DLL was unloaded. The NotificationData parameter points to an <see cref="LDR_DLL_UNLOADED_NOTIFICATION_DATA"/> structure.
        /// </summary>
        UNLOADED = 2
    }
}
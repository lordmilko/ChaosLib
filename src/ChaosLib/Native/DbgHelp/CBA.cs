namespace ChaosLib
{
    public enum CBA : uint
    {
        /// <summary>
        /// Deferred symbol load has started.<para/>
        /// The CallbackData parameter is a pointer to a <see cref="IMAGEHLP_DEFERRED_SYMBOL_LOAD64"/> structure.
        /// </summary>
        CBA_DEFERRED_SYMBOL_LOAD_START = 0x00000001,

        /// <summary>
        /// Deferred symbol load has completed.<para/>
        /// The CallbackData parameter is a pointer to a <see cref="IMAGEHLP_DEFERRED_SYMBOL_LOAD64"/> structure.
        /// </summary>
        CBA_DEFERRED_SYMBOL_LOAD_COMPLETE = 0x00000002,

        /// <summary>
        /// Deferred symbol load has failed.<para/>
        /// The CallbackData parameter is a pointer to a <see cref="IMAGEHLP_DEFERRED_SYMBOL_LOAD64"/> structure. The symbol handler will attempt to load the symbols again if the callback function sets the FileName member of this structure.
        /// </summary>
        CBA_DEFERRED_SYMBOL_LOAD_FAILURE = 0x00000003,

        /// <summary>
        /// Symbols have been unloaded.<para/>
        /// The CallbackData parameter should be ignored.
        /// </summary>
        CBA_SYMBOLS_UNLOADED = 0x00000004,

        /// <summary>
        /// Duplicate symbols were found. This reason is used only in COFF or CodeView format.<para/>
        /// The CallbackData parameter is a pointer to a <see cref="IMAGEHLP_DUPLICATE_SYMBOL64"/> structure. To specify which symbol to use, set the SelectedSymbol member of this structure.
        /// </summary>
        CBA_DUPLICATE_SYMBOL = 0x00000005,

        /// <summary>
        /// The loaded image has been read.<para/>
        /// The CallbackData parameter is a pointer to a <see cref="IMAGEHLP_CBA_READ_MEMORY"/> structure. The callback function should read the number of bytes specified by the bytes member into the buffer specified by the buf member, and update the bytesread member accordingly.
        /// </summary>
        CBA_READ_MEMORY = 0x00000006,

        /// <summary>
        /// Deferred symbol loading has started. To cancel the symbol load, return TRUE.<para/>
        /// The CallbackData parameter is a pointer to a <see cref="IMAGEHLP_DEFERRED_SYMBOL_LOAD64"/> structure.
        /// </summary>
        CBA_DEFERRED_SYMBOL_LOAD_CANCEL = 0x00000007,

        /// <summary>
        /// Symbol options have been updated. To retrieve the current options, call the SymGetOptions function.<para/>
        /// The CallbackData parameter should be ignored.
        /// </summary>
        CBA_SET_OPTIONS = 0x00000008,

        /// <summary>
        /// Display verbose information. If you do not handle this event, the information is resent through the <see cref="CBA_DEBUG_INFO"/> event.<para/>
        /// The CallbackData parameter is a pointer to a <see cref="IMAGEHLP_CBA_EVENT"/> structure.
        /// </summary>
        CBA_EVENT = 0x00000010,

        /// <summary>
        /// Deferred symbol load has partially completed. The symbol loader is unable to read the image header from either the image file or the specified module.<para/>
        /// The CallbackData parameter is a pointer to a <see cref="IMAGEHLP_DEFERRED_SYMBOL_LOAD64"/> structure. The symbol handler will attempt to load the symbols again if the callback function sets the FileName member of this structure.
        /// </summary>
        CBA_DEFERRED_SYMBOL_LOAD_PARTIAL = 0x00000020,

        /// <summary>
        /// Display verbose information.<para/>
        /// The CallbackData parameter is a pointer to a string.
        /// </summary>
        CBA_DEBUG_INFO = 0x10000000,

        /// <summary>
        /// Display verbose information for source server.<para/>
        /// The CallbackData parameter is a pointer to a string.
        /// </summary>
        CBA_SRCSRV_INFO = 0x20000000,

        /// <summary>
        /// Display verbose information for source server. If you do not handle this event, the information is resent through the <see cref="CBA_DEBUG_INFO"/> event.<para/>
        /// The CallbackData parameter is a pointer to a <see cref="IMAGEHLP_CBA_EVENT"/> structure.
        /// </summary>
        CBA_SRCSRV_EVENT = 0x40000000,

        CBA_UPDATE_STATUS_BAR = 0x50000000,
        CBA_ENGINE_PRESENT = 0x60000000,
        CBA_CHECK_ENGOPT_DISALLOW_NETWORK_PATHS = 0x70000000,
        CBA_CHECK_ARM_MACHINE_THUMB_TYPE_OVERRIDE = 0x80000000,
        CBA_XML_LOG = 0x90000000,
        CBA_MAP_JIT_SYMBOL = 0xA0000000
    }
}
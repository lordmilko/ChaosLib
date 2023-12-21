using System;

namespace ChaosLib
{
    [Flags]
    public enum ACCESS_MASK
    {
        /// <summary>
        /// Query access to the directory object.
        /// </summary>
        DIRECTORY_QUERY = 1,

        /// <summary>
        /// Name-lookup access to the directory object.
        /// </summary>
        DIRECTORY_TRAVERSE = 2,

        /// <summary>
        /// Name-creation access to the directory object.
        /// </summary>
        DIRECTORY_CREATE_OBJECT = 4,

        /// <summary>
        /// Subdirectory-creation access to the directory object.
        /// </summary>
        DIRECTORY_CREATE_SUBDIRECTORY = 8
    }
}
﻿using System.Linq;

namespace ChaosLib.Metadata
{
    /// <summary>
    /// Encapsulates the enture resource hierarchy descended from the root IMAGE_RESOURCE_DIRECTORY entry that may have been read from IMAGE_OPTIONAL_HEADER.DataDirectory[IMAGE_DIRECTORY_ENTRY_RESOURCE]<para/>
    /// and provides access to specified types of resources that may have been read out of specified IMAGE_RESOURCE_DATA_ENTRY structures contained within this hierarchy.
    /// </summary>
    public interface IImageResourceDirectoryInfo
    {
        IImageResourceDirectoryLevel Root { get; }

        IVS_VERSIONINFO Version { get; }
    }

    /// <summary>
    /// Encapsulates the enture resource hierarchy descended from the root IMAGE_RESOURCE_DIRECTORY entry that may have been read from IMAGE_OPTIONAL_HEADER.DataDirectory[IMAGE_DIRECTORY_ENTRY_RESOURCE]<para/>
    /// and provides access to specified types of resources that may have been read out of specified IMAGE_RESOURCE_DATA_ENTRY structures contained within this hierarchy.
    /// </summary>
    public class ImageResourceDirectoryInfo : IImageResourceDirectoryInfo
    {
        public IImageResourceDirectoryLevel Root { get; }

        public IVS_VERSIONINFO Version { get; }

        internal ImageResourceDirectoryInfo(PEFile file, PEBinaryReader reader)
        {
            Root = ImageResourceDirectoryLevel.New(file, reader);

            Version = ReadVersionInfo(file, reader);
        }

        private VS_VERSIONINFO ReadVersionInfo(PEFile file, PEBinaryReader reader)
        {
            const int VS_FILE_INFO = 16;
            const int VS_VERSION_INFO = 1;

            var entry = Root[VS_FILE_INFO]?[VS_VERSION_INFO]?.Directory.Entries.FirstOrDefault();

            if (entry == null)
                return null;

            int offset;

            if (!file.TryGetOffset(entry.Data.OffsetToData, out offset))
                return null;

            reader.Seek(offset);

            var version = new VS_VERSIONINFO(reader);

            if (version.Value.Signature != VS_FIXEDFILEINFO.FixedFileInfoSignature)
                return null;

            return version;
        }
    }
}

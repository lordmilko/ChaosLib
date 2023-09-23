﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ChaosLib.Metadata
{
    /// <summary>
    /// Encapsulates the IMAGE_DEBUG_DIRECTORY entries that have been read from IMAGE_OPTIONAL_HEADER.DataDirectory[IMAGE_DIRECTORY_ENTRY_DEBUG]
    /// and provides access to information that is pointed to by various different IMAGE_DEBUG_DIRECTORY types.
    /// </summary>
    public interface IImageDebugDirectoryInfo
    {
        ICodeViewInfo[] CodeViews { get; }

        IImageDebugDirectory[] Entries { get; }
    }

    /// <summary>
    /// Encapsulates the IMAGE_DEBUG_DIRECTORY entries that have been read from IMAGE_OPTIONAL_HEADER.DataDirectory[IMAGE_DIRECTORY_ENTRY_DEBUG]
    /// and provides access to information that is pointed to by various different IMAGE_DEBUG_DIRECTORY types.
    /// </summary>
    public class ImageDebugDirectoryInfo : IImageDebugDirectoryInfo
    {
        private PEFile file;

        public ICodeViewInfo[] CodeViews { get; }

        public IImageDebugDirectory[] Entries { get; }

        internal ImageDebugDirectoryInfo(PEFile file, PEBinaryReader reader)
        {
            this.file = file;

            Entries = ReadDebugDirectories(reader);
            CodeViews = Entries
                .Where(v => v.Type == ImageDebugType.CodeView)
                .Select(v =>
                {
                    var offset = file.GetOffset(v);

                    reader.Seek(offset);

                    return (ICodeViewInfo) new CodeViewInfo(reader);
                })
                .ToArray();
        }

        private IImageDebugDirectory[] ReadDebugDirectories(PEBinaryReader reader)
        {
            var entryCount = file.OptionalHeader.DebugTableDirectory.Size / ImageDebugDirectory.Size;

            if (entryCount > 0)
            {
                int offset;

                if (!file.TryGetDirectoryOffset(file.OptionalHeader.DebugTableDirectory, out offset, true))
                    return Array.Empty<IImageDebugDirectory>();

                reader.Seek(offset);

                var list = new List<IImageDebugDirectory>();

                for (var i = 0; i < entryCount; i++)
                    list.Add(new ImageDebugDirectory(reader));

                return list.ToArray();
            }

            return Array.Empty<IImageDebugDirectory>();
        }
    }
}

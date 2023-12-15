using System;

namespace ChaosLib.Metadata
{
    public interface IImageImportDescriptorInfo
    {
        string Name { get; }
    }

    public class ImageImportDescriptorInfo : IImageImportDescriptorInfo
    {
        public string Name { get; }

        public ImageImportDescriptorInfo(ImageImportDescriptor descriptor, PEFile file, PEBinaryReader reader)
        {
            if (!file.TryGetOffset(descriptor.Name, out var nameOffset))
                throw new InvalidOperationException($"Failed to get offset of import {descriptor.Name}");

            reader.Seek(nameOffset);
            Name = reader.ReadSZString();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
using System;
using System.Collections.Generic;

namespace ChaosLib.Metadata
{
    public interface IImageExportDirectory
    {
        int Characteristics { get; }
        int TimeDateStamp { get; }
        ushort MajorVersion { get; }
        ushort MinorVersion { get; }
        int Name { get; }
        int Base { get; }
        int NumberOfFunctions { get; }
        int NumberOfNames { get; }
        int AddressOfFunctions { get; }
        int AddressOfNames { get; }
        int AddressOfNameOrdinals { get; }

        IImageExportInfo[] Exports { get; }
    }

    /// <summary>
    /// Represents the IMAGE_EXPORT_DIRECTORY structure, that describes the locations and components of the export table within the image.
    /// </summary>
    public class ImageExportDirectory : IImageExportDirectory
    {
        public int Characteristics { get; }
        public int TimeDateStamp { get; }
        public ushort MajorVersion { get; }
        public ushort MinorVersion { get; }
        public int Name { get; }
        public int Base { get; }
        public int NumberOfFunctions { get; }
        public int NumberOfNames { get; }
        public int AddressOfFunctions { get; }
        public int AddressOfNames { get; }
        public int AddressOfNameOrdinals { get; }

        public IImageExportInfo[] Exports { get; }

        public static int Size =
            sizeof(int) + //Characteristics
            sizeof(int) + //TimeDateStamp
            sizeof(ushort) + //MajorVersion
            sizeof(ushort) + //MinorVersion
            sizeof(int) + //Name
            sizeof(int) + //Base
            sizeof(int) + //NumberOfFunctions
            sizeof(int) + //NumberOfNames
            sizeof(int) + //AddressOfFunctions
            sizeof(int) + //AddressOfNames
            sizeof(int); //AddressOfNameOrdinals

        internal ImageExportDirectory(PEFile file, PEBinaryReader reader)
        {
            Characteristics = reader.ReadInt32();
            TimeDateStamp = reader.ReadInt32();
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            Name = reader.ReadInt32();
            Base = reader.ReadInt32();
            NumberOfFunctions = reader.ReadInt32();
            NumberOfNames = reader.ReadInt32();
            AddressOfFunctions = reader.ReadInt32();
            AddressOfNames = reader.ReadInt32();
            AddressOfNameOrdinals = reader.ReadInt32();

            Exports = ReadExports(file, reader);
        }

        private IImageExportInfo[] ReadExports(PEFile file, PEBinaryReader reader)
        {
            if (!file.TryGetOffset(AddressOfFunctions, out var addressOfFunctionsOffset))
                return Array.Empty<IImageExportInfo>();

            if (!file.TryGetOffset(AddressOfNames, out var addressOfNamesOffset))
                return Array.Empty<IImageExportInfo>();

            var exports = new List<IImageExportInfo>();

            var exportTableStart = file.OptionalHeader.ExportTableDirectory.RelativeVirtualAddress;
            var exportTableEnd = exportTableStart + file.OptionalHeader.ExportTableDirectory.Size;

            var functionAddresses = new int[NumberOfFunctions];
            var functionNameAddresses = new int[NumberOfNames];
            var functionOrdinals = new ushort[NumberOfNames];

            //Get the function addresses
            reader.Seek(addressOfFunctionsOffset);

            for (var i = 0; i < NumberOfFunctions; i++)
                functionAddresses[i] = reader.ReadInt32();

            //Get the function names
            reader.Seek(addressOfNamesOffset);

            for (var i = 0; i < NumberOfNames; i++)
                functionNameAddresses[i] = reader.ReadInt32();

            //Get the function ordinals
            reader.Seek(AddressOfNameOrdinals);

            for (var i = 0; i < NumberOfNames; i++)
                functionOrdinals[i] = reader.ReadUInt16();

            for (var i = 0; i < NumberOfNames; i++)
            {
                var functionAddress = functionAddresses[i];
                var nameAddress = functionNameAddresses[i];
                var ordinal = functionOrdinals[i];
                var ordinalPlusBase = ordinal + Base;

                reader.Seek(nameAddress);
                var name = reader.ReadSZString();

                if (ordinalPlusBase != i)
                {
                    /* Most likely there are some hidden functions that are exported by ordinal only (such as in kernel32.dll,
                     * resulting in all functions being off by 1-2). Get the true function that is pointed to by the ordinal,
                     * which may or may not also be a forwarded export. */

                    functionAddress = functionAddresses[ordinal];
                }

                //If the actual address of the function is within the bounds of the export table (rather than a random place
                //in the module) that means that a. addressOfFunction is pointing to a string and b. function actually comes
                //from another module. Lookup _that_ external module.
                //https://reverseengineering.stackexchange.com/questions/16023/exports-that-redirects-to-other-library
                if (functionAddress >= exportTableStart && functionAddress <= exportTableEnd)
                {
                    reader.Seek(functionAddress);
                    var redirectName = reader.ReadSZString();

                    exports.Add(new ImageForwardedExportInfo(
                        name,
                        i,
                        redirectName,
                        ordinalPlusBase
                    ));
                }
                else
                {
                    exports.Add(
                        new ImageExportInfo(
                            name,
                            i,
                            (IntPtr) (file.OptionalHeader.ImageBase + (uint) functionAddress),
                            ordinalPlusBase
                        )
                    );
                }
            }

            return exports.ToArray();
        }
    }
}

using System;
using System.Collections.Generic;

namespace ChaosLib.Metadata
{
    public interface IImageExportDirectoryInfo
    {
        int Characteristics { get; }
        int TimeDateStamp { get; }
        ushort MajorVersion { get; }
        ushort MinorVersion { get; }
        string Name { get; }
        int Base { get; }
        IImageExportInfo[] Exports { get; }
    }

    public class ImageExportDirectoryInfo : IImageExportDirectoryInfo
    {
        public int Characteristics { get; }
        public int TimeDateStamp { get; }
        public ushort MajorVersion { get; }
        public ushort MinorVersion { get; }
        public string Name { get; }
        public int Base { get; }

        public IImageExportInfo[] Exports { get; }

        internal ImageExportDirectoryInfo(ImageExportDirectory exportDirectory, PEFile file, PEBinaryReader reader)
        {
            if (!file.TryGetOffset(exportDirectory.Name, out var nameOffset))
                throw new InvalidOperationException($"Failed to get offset of export directory name {exportDirectory.Name}");

            reader.Seek(nameOffset);
            Name = reader.ReadSZString();

            Characteristics = exportDirectory.Characteristics;
            TimeDateStamp = exportDirectory.TimeDateStamp;
            MajorVersion = exportDirectory.MajorVersion;
            MinorVersion = exportDirectory.MinorVersion;
            Base = exportDirectory.Base;;

            Exports = ReadExports(exportDirectory, file, reader);
        }

        private IImageExportInfo[] ReadExports(ImageExportDirectory exportDirectory, PEFile file, PEBinaryReader reader)
        {
            if (!file.TryGetOffset(exportDirectory.AddressOfFunctions, out var addressOfFunctionsOffset))
                return Array.Empty<IImageExportInfo>();

            if (!file.TryGetOffset(exportDirectory.AddressOfNames, out var addressOfNamesOffset))
                return Array.Empty<IImageExportInfo>();

            if (!file.TryGetOffset(exportDirectory.AddressOfNameOrdinals, out var addressOfNameOrdinalsOffset))
                return Array.Empty<IImageExportInfo>();

            var exports = new List<IImageExportInfo>();

            var exportTableStart = file.OptionalHeader.ExportTableDirectory.RelativeVirtualAddress;
            var exportTableEnd = exportTableStart + file.OptionalHeader.ExportTableDirectory.Size;

            var functionAddresses = new int[exportDirectory.NumberOfFunctions];
            var functionNameAddresses = new int[exportDirectory.NumberOfNames];
            var functionOrdinals = new ushort[exportDirectory.NumberOfNames];

            //Get the function addresses
            reader.Seek(addressOfFunctionsOffset);

            for (var i = 0; i < exportDirectory.NumberOfFunctions; i++)
                functionAddresses[i] = reader.ReadInt32();

            //Get the function names
            reader.Seek(addressOfNamesOffset);

            for (var i = 0; i < exportDirectory.NumberOfNames; i++)
                functionNameAddresses[i] = reader.ReadInt32();

            //Get the function ordinals
            reader.Seek(addressOfNameOrdinalsOffset);

            for (var i = 0; i < exportDirectory.NumberOfNames; i++)
                functionOrdinals[i] = reader.ReadUInt16();

            for (var i = 0; i < exportDirectory.NumberOfNames; i++)
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
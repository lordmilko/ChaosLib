using ClrDebug;

namespace ChaosLib.Metadata
{
    /// <summary>
    /// Provides facilities for reading .NET metadata signatures.
    /// </summary>
    public interface ISigReader
    {
        /// <summary>
        /// Reads the metadata signature of a specified .NET method.
        /// </summary>
        /// <param name="token">A token that identifies the method whose signature should be read.</param>
        /// <param name="import">The <see cref="MetaDataImport"/> that should be used for resolving enhanced metadata info.</param>
        /// <param name="props">An optional set of properties for the specified method that have already been retrieved from the <see cref="MetaDataImport"/>.</param>
        /// <returns>A type that describes the method signature.</returns>
        ISigMethod ReadMethod(mdMethodDef token, MetaDataImport import, MetaDataImport_GetMethodPropsResult? props = null);

        ISigField ReadField(mdFieldDef token, MetaDataImport import, GetFieldPropsResult? props = null);

        ISigCustomAttribute ReadCustomAttribute(mdCustomAttribute token, MetaDataImport import, ITypeRefResolver typeRefResolver);
    }

    public class SigReader : ISigReader
    {
        public ISigMethod ReadMethod(mdMethodDef token, MetaDataImport import, MetaDataImport_GetMethodPropsResult? props = null)
        {
            var info = props ?? import.GetMethodProps(token);

            var reader = new SigReaderInternal(info.ppvSigBlob, info.pcbSigBlob, token, import);

            return reader.ParseMethod(info.szMethod, true);
        }

        public ISigField ReadField(mdFieldDef token, MetaDataImport import, GetFieldPropsResult? props = null)
        {
            var info = props ?? import.GetFieldProps(token);

            var reader = new SigReaderInternal(info.ppvSigBlob, info.pcbSigBlob, token, import);

            return reader.ParseField(info.szField);
        }

        public ISigCustomAttribute ReadCustomAttribute(mdCustomAttribute token, MetaDataImport import, ITypeRefResolver typeRefResolver)
        {
            var info = import.GetCustomAttributeProps(token);

            var reader = new SigReaderInternal(info.ppBlob, info.pcbSize, token, import);

            return reader.ParseCustomAttribute(info.ptkType, typeRefResolver);
        }
    }
}

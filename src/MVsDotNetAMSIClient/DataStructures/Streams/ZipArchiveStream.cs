using ICSharpCode.SharpZipLib.Zip;

namespace MVsDotNetAMSIClient.DataStructures.Streams
{
    internal class ZipArchiveStream : BaseInputStream<ZipInputStream>
    {
        ZipEntry zipEntry;
        readonly bool throwOnCryptedEntries;

        public override string CurrentEntryName => zipEntry?.Name;

        internal ZipArchiveStream(string filePath, int blockSize, bool throwOnEnryptedEntries)
            : base(filePath, blockSize)
        {
            this.throwOnCryptedEntries = throwOnEnryptedEntries;
            ReadStream = new ZipInputStream(fileStream, blockSize);
            MoveToNextEntry();
        }

        protected override bool MoveToNextEntry()
        {
            do
            {
                if ((zipEntry = ReadStream.GetNextEntry()) == null)
                    return false;

                if (throwOnCryptedEntries && zipEntry.IsCrypted)
                    throw AMSIException.ZipContainsCryptedEntry(zipEntry.Name);
            }
            while (!zipEntry.IsFile);

            return true;
        }

        public override void Dispose()
        {
            ReadStream?.Dispose();
            fileStream?.Dispose();
        }
    }
}

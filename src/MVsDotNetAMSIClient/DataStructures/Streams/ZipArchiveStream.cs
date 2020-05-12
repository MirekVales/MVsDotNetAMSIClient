using ICSharpCode.SharpZipLib.Zip;

namespace MVsDotNetAMSIClient.DataStructures.Streams
{
    internal class ZipArchiveStream : BaseInputStream<ZipInputStream>
    {
        ZipEntry zipEntry;
        readonly bool throwOnEncryptedEntries;

        public override string CurrentEntryName => zipEntry?.Name;

        internal ZipArchiveStream(string filePath, int blockSize, bool throwOnEncryptedEntries)
            : base(filePath, blockSize)
        {
            this.throwOnEncryptedEntries = throwOnEncryptedEntries;
            ReadStream = new ZipInputStream(fileStream, blockSize);
            MoveToNextEntry();
        }

        protected override bool MoveToNextEntry()
        {
            do
            {
                if ((zipEntry = ReadStream.GetNextEntry()) == null)
                    return false;

                if (throwOnEncryptedEntries && zipEntry.IsCrypted)
                    throw AMSIRejectedByPolicyException.ZipContainsCryptedEntry(zipEntry.Name);
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

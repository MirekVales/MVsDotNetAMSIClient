using ICSharpCode.SharpZipLib.Tar;

namespace MVsDotNetAMSIClient.DataStructures.Streams
{
    internal class TarArchiveStream : BaseInputStream<TarInputStream>
    {
        TarEntry tarEntry;

        public override string CurrentEntryName => tarEntry?.Name;

        internal TarArchiveStream(string filePath, int blockSize)
            : base(filePath, blockSize)
        {
            ReadStream = new TarInputStream(fileStream, blockSize);
            MoveToNextEntry();
        }

        protected override bool MoveToNextEntry()
        {
            do
            {
                if ((tarEntry = ReadStream.GetNextEntry()) == null)
                    return false;
            }
            while (tarEntry.IsDirectory);

            return true;
        }

        public override void Dispose()
        {
            ReadStream?.Dispose();
            fileStream?.Dispose();
        }
    }
}

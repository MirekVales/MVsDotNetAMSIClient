using System;
using System.IO;

namespace MVsDotNetAMSIClient.DataStructures.Streams
{
    internal class InputStream : BaseInputStream<Stream>
    {
        public override string CurrentEntryName => fileStream.Name;

        internal InputStream(string filePath, int blockSize)
            : base(filePath, blockSize)
            => ReadStream = fileStream;

        internal InputStream(string filePath, int blockSize, Func<Stream, Stream> readStreamFunc)
            : base(filePath, blockSize)
            => ReadStream = readStreamFunc(fileStream);

        protected override bool MoveToNextEntry()
           => false;

        public override void Dispose()
        {
            ReadStream?.Dispose();
            fileStream?.Dispose();
        }
    }
}

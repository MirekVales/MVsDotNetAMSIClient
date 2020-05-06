using System;
using System.IO;

namespace MVsDotNetAMSIClient.DataStructures.Streams
{
    internal abstract class BaseInputStream<TStream> : IInputStream
        where TStream : Stream
    {
        protected readonly FileStream fileStream;
        public TStream ReadStream { get; protected set; }

        public long Position { get; protected set; }

        public abstract string CurrentEntryName { get; }

        internal BaseInputStream(string filePath, int blockSize)
        {
            fileStream = new FileStream(
                filePath
                , FileMode.Open
                , FileAccess.Read
                , FileShare.Read
                , blockSize
                , FileOptions.SequentialScan);
        }

        public int Read(byte[] buffer, int numberOfBytes)
        {
            var readBytes = ReadBytes(buffer, numberOfBytes);

            if (readBytes > 0)
                return readBytes;

            if (readBytes == 0 && !MoveToNextEntry())
                return 0;

            return ReadBytes(buffer, numberOfBytes);
        }

        public void ScanTo(byte[] buffer, long offset)
        {
            if (ReadStream.CanSeek)
            {
                ReadStream.Seek(offset, SeekOrigin.Begin);
                return;
            }

            var diff = offset - Position;
            while (diff > 0 && (_ = Read(buffer, (int)Math.Min(diff, buffer.Length))) > 0)
                diff = offset - Position;

            if (Position != offset)
                throw new IOException($"Failed to navigate to position {offset} in file {fileStream.Name}");
        }

        protected virtual int ReadBytes(byte[] buffer, int numberOfBytes)
        {
            var readBytes = ReadStream.Read(buffer, 0, numberOfBytes);
            Position += readBytes;
            return readBytes;
        }

        protected abstract bool MoveToNextEntry();

        public abstract void Dispose();
    }
}

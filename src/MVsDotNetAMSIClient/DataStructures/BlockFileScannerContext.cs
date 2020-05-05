using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class BlockFileScannerContext : IDisposable
    {
        internal string FilePath { get; set; }
        internal FileStream FileStream { get; set; }
        internal Stream ReadStream { get; set; }
        internal byte[] Buffer { get; set; }
        internal Task<string> MD5Hash { get; set; }
        internal int BlockSize { get; set; }
        internal FileSignature FileSignature { get; set; }
        internal bool ScanOverlaps { get; set; }
        internal bool InspectZipFiles { get; set; }

        internal BlockFileScannerContext(string filePath, int blockSize, bool scanOverlaps, bool inspectZipFiles)
        {
            FilePath = filePath;
            BlockSize = blockSize;
            ScanOverlaps = scanOverlaps;
            InspectZipFiles = inspectZipFiles;

            Buffer = new byte[blockSize];
            FileSignature = inspectZipFiles ? FileSignatureReader.GetFileSignature(filePath) : FileSignature.Unknown;
            MD5Hash = Task.Run(() => new FileInfo(filePath).GetFileMD5Hash());

            InitiateStreams();
        }

        internal void InitiateStreams()
        {
            ReadStream?.Dispose();
            FileStream?.Dispose();

            FileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, BlockSize, FileOptions.SequentialScan);
            ReadStream = DecompressStreams[FileSignature.FileType](FileStream);
        }

        Dictionary<FileType, Func<FileStream, Stream>> DecompressStreams => new Dictionary<FileType, Func<FileStream, Stream>>()
            {
                { FileType.Unknown, fileStream => fileStream },
                { FileType.Bzip2, fileStream => new BZip2InputStream(fileStream) },
                { FileType.GZip, fileStream => new GZipInputStream(fileStream) },
                { FileType.Tar, fileStream => new TarInputStream(fileStream) },
                { FileType.Zip, fileStream => new ZipInputStream(fileStream) },
            };

        internal IEnumerable<long> GetOverlaps(long streamLength)
            => Enumerable.Range(1, Math.Max(0, (int)Math.Ceiling((double)streamLength / BlockSize) - 1))
                .Select(index => (long)index * BlockSize - BlockSize / 2);

        internal void ScanToPosition(long offset)
        {
            var diff = offset - ReadStream.Position;
            while (diff > 0 && (_ = ReadStream.Read(Buffer, 0, (int)Math.Min(diff, Buffer.Length))) > 0)
                diff = offset - ReadStream.Position;

            if (ReadStream.Position != offset)
                throw new IOException($"Failed to navigate to position {offset} in file {FilePath}");
        }

        public void Dispose()
        {
            ReadStream?.Dispose();
            FileStream?.Dispose();
        }
    }
}

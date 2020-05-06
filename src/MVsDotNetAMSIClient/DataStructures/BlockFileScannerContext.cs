using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;
using MVsDotNetAMSIClient.DataStructures.Streams;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class BlockFileScannerContext
    {
        internal string FilePath { get; }
        internal FileInfo FileInfo { get; }
        internal byte[] Buffer { get; }
        internal Task<string> MD5Hash { get; }
        internal int BlockSize { get; }
        internal FileSignature FileSignature { get; }
        internal bool ScanOverlaps { get; }
        internal bool InspectZipFiles { get; }
        internal long? MaxArchiveSize { get; }
        internal bool AcceptEncryptedZipEntries { get; }

        internal BlockFileScannerContext(
            string filePath
            , int blockSize
            , bool scanOverlaps
            , bool inspectZipFiles
            , long? maxArchiveSize
            , bool acceptCryptedZipEntries)
        {
            FilePath = filePath;
            FileInfo = new FileInfo(filePath);
            BlockSize = blockSize;
            ScanOverlaps = scanOverlaps;
            InspectZipFiles = inspectZipFiles;
            MaxArchiveSize = maxArchiveSize;
            AcceptEncryptedZipEntries = acceptCryptedZipEntries;

            Buffer = new byte[blockSize];
            FileSignature = inspectZipFiles ? FileSignatureReader.GetFileSignature(filePath) : FileSignature.Unknown;
            MD5Hash = Task.Run(() => FileInfo.GetFileMD5Hash());
        }

        internal IInputStream InitiateStream(FileType fileType)
            => InputStreamsFunc[fileType]();

        Dictionary<FileType, Func<IInputStream>> InputStreamsFunc => new Dictionary<FileType, Func<IInputStream>>()
            {
                { FileType.Unknown, () => new InputStream(FilePath, BlockSize) },
                { FileType.Bzip2, () => new InputStream(FilePath, BlockSize, fileStream => new BZip2InputStream(fileStream)) },
                { FileType.GZip, () => new InputStream(FilePath, BlockSize, fileStream => new GZipInputStream(fileStream, BlockSize)) },
                { FileType.Tar, () => new TarArchiveStream(FilePath, BlockSize) },
                { FileType.Zip, () => new ZipArchiveStream(FilePath, BlockSize, !AcceptEncryptedZipEntries) },
            };

        internal IEnumerable<long> GetOverlaps(long streamLength)
            => Enumerable.Range(1, Math.Max(0, (int)Math.Ceiling((double)streamLength / BlockSize) - 1))
                .Select(index => (long)index * BlockSize - BlockSize / 2);

        internal bool IsArchive
            => new[] { FileType.Bzip2, FileType.GZip, FileType.Tar, FileType.Zip }.Contains(FileSignature.FileType);

        internal bool ExceedsMaxArchiveSize
            => IsArchive && MaxArchiveSize.HasValue && MaxArchiveSize < FileInfo.Length;
    }
}

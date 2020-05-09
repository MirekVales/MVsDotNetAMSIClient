using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.DataStructures.Streams;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class FileStreamScanner
    {
        readonly AMSIClient client;
        readonly string filePath;
        readonly FileInfo fileInfo;
        readonly byte[] buffer;
        readonly int blockSize;
        readonly Task<string> md5Hash;
        readonly FileSignature fileSignature;
        readonly bool acceptEncryptedZipEntries;

        internal FileStreamScanner(
            AMSIClient client
            , string filePath
            , int blockSize
            , bool acceptEncryptedZipEntries
        )
        {
            this.client = client;

            this.filePath = filePath;
            fileInfo = new FileInfo(filePath);
            this.blockSize = blockSize;
            this.acceptEncryptedZipEntries = acceptEncryptedZipEntries;

            buffer = new byte[blockSize];
            fileSignature = new FileSignatureReader().GetFileSignature(filePath);
            md5Hash = Task.Run(() => fileInfo.GetFileMD5Hash());
        }

        internal ScanResult GetRejectedResult(string reason)
        {
            using (var builder = new ResultBuilder(new ScanContext(
                client
                , null
                , filePath
                , ContentType.File
                , fileSignature.FileType
                , fileInfo.Length
                , null)))
                return builder.ToResultRejected(reason);
        }

        internal IInputStream InitiateStream(FileType fileType)
            => InputStreamsFunc[fileType]();

        IDictionary<FileType, Func<IInputStream>> InputStreamsFunc
            => new Dictionary<FileType, Func<IInputStream>>()
        {
            { FileType.Unknown, () => new InputStream(filePath, blockSize)},
            { FileType.Executable, () => new InputStream(filePath, blockSize)},
            { FileType.MicrosoftOfficeDocument, () => new InputStream(filePath, blockSize)},
            { FileType.CompoundFileBinary, () => new InputStream(filePath, blockSize)},
            { FileType.PDF, () => new InputStream(filePath, blockSize)},
            { FileType.Bzip2, () => new InputStream(filePath, blockSize, fileStream => new BZip2InputStream(fileStream))},
            { FileType.GZip, () => new InputStream(filePath, blockSize, fileStream => new GZipInputStream(fileStream, blockSize))},
            { FileType.Tar, () => new TarArchiveStream(filePath, blockSize)},
            { FileType.Zip, () => new ZipArchiveStream(filePath, blockSize, !acceptEncryptedZipEntries)}
        };

        internal IEnumerable<long> GetOverlaps(long streamLength)
            => Enumerable.Range(1, Math.Max(0, (int)Math.Ceiling((double)streamLength / blockSize) - 1))
                .Select(index => (long)index * blockSize - blockSize / 2);

        internal bool IsArchive
            => new[] { FileType.Bzip2, FileType.GZip, FileType.Tar, FileType.Zip }.Contains(fileSignature.FileType);

        internal bool ExceedsMaxFileSize(long? maxSize)
            => maxSize.HasValue && maxSize < fileInfo.Length;

        internal bool ExceedsMaxArchiveSize(long? maxArchiveSize)
            => IsArchive && maxArchiveSize.HasValue && maxArchiveSize < fileInfo.Length;

        internal bool TryScanArchiveOrBinary(bool scanOverlaps, out ScanResult lastResult, out ScanResult breakingResult)
            => TryScan(fileSignature.FileType, scanOverlaps, out lastResult, out breakingResult);

        internal bool TryScanBinary(bool scanOverlaps, out ScanResult lastResult, out ScanResult breakingResult)
            => TryScan(FileType.Unknown, scanOverlaps, out lastResult, out breakingResult);

        bool TryScan(FileType fileType, bool scanOverlaps, out ScanResult lastResult, out ScanResult breakingResult)
        {
            ScanResult anyResult = null;

            try
            {
                breakingResult = ScanStream(fileType, scanOverlaps)
                        .FirstOrDefault(segmentResult => ResultBuilder.IsBreakResult((anyResult = segmentResult).Result));
            }
            catch (Exception e)
            {
                breakingResult = new ScanResult()
                {
                    Result = DetectionResult.ApplicationError
                    ,
                    ResultDetail = e.ToString()
                };
            }

            if (anyResult != null)
            {
                anyResult.ContentByteSize = streamLength;
                anyResult.ContentHash = md5Hash.GetAwaiter().GetResult();
                anyResult.ContentName = filePath;
                anyResult.ContentType = ContentType.File;
                anyResult.ContentFileType = fileSignature.FileType;
            }

            lastResult = anyResult;
            return breakingResult == null;
        }

        long streamLength = 0;

        IEnumerable<ScanResult> ScanStream(FileType fileType, bool scanOverlaps)
        {
            streamLength = 0L;
            using (var stream = InitiateStream(fileType))
            {
                int read;
                do
                {
                    read = stream.Read(buffer, blockSize);
                    var scanResult = client.ScanBuffer(
                        buffer
                        , (uint)blockSize
                        , $"{filePath}@{stream.Position}>{stream.CurrentEntryName}");

                    yield return scanResult;

                } while (read > 0);
                streamLength = stream.Position;
            }

            if (scanOverlaps && fileInfo.Length > blockSize)
                using (var stream = InitiateStream(fileType))
                    foreach (var overlap in GetOverlaps(streamLength))
                        yield return ScanSegment(stream, overlap);
        }

        ScanResult ScanSegment(IInputStream stream, long offset)
        {
            stream.ScanTo(buffer, offset);
            stream.Read(buffer, blockSize);
            return client.ScanBuffer(buffer, (uint)blockSize, $"{filePath}@{offset}");
        }
    }
}

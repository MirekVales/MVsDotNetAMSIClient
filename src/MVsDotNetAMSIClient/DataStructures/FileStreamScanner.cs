using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.Contracts.Enums;
using MVsDotNetAMSIClient.DetailProviders;
using MVsDotNetAMSIClient.DataStructures.Streams;
using MVsFileTypes.Analysis;
using MVsFileTypes.Predefined;
using MVsFileTypes.Contracts;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class FileStreamScanner : IDisposable
    {
        readonly AMSIClient client;
        readonly string filePath;
        readonly FileInfo fileInfo;
        readonly byte[] buffer;
        readonly int blockSize;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly Task<string> md5Hash;
        readonly MVsFileTypes.Contracts.FileSignature fileSignature;
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
            cancellationTokenSource = new CancellationTokenSource();
            using (var signatureReader = new FileSignatureReader(filePath, Signatures.Get()))
                fileSignature = signatureReader.GetFileSignature();
            md5Hash = client.Configuration.SkipContentHashing
                ? Task.FromResult((string)null)
                : Task.Run(() => fileInfo.GetFileMD5Hash(), cancellationTokenSource.Token);
        }

        internal ScanResult GetRejectedResult(string reason)
        {
            using (var builder = new ResultBuilder(new ScanContext(
                client
                , null
                , filePath
                , ContentType.File
                , ConvertFileType(fileSignature.FileType)
                , fileInfo.Length
                , null)))
                return builder.ToResult(DetectionResult.FileRejected, reason);
        }

        internal IInputStream InitiateStream(Contracts.Enums.FileType fileType)
            => InputStreamsFunc[fileType]();

        IDictionary<Contracts.Enums.FileType, Func<IInputStream>> InputStreamsFunc
            => new Dictionary<Contracts.Enums.FileType, Func<IInputStream>>()
        {
            { Contracts.Enums.FileType.Unknown, () => new InputStream(filePath, blockSize)},
            { Contracts.Enums.FileType.Executable, () => new InputStream(filePath, blockSize)},
            { Contracts.Enums.FileType.MicrosoftOfficeDocument, () => new InputStream(filePath, blockSize)},
            { Contracts.Enums.FileType.CompoundFileBinary, () => new InputStream(filePath, blockSize)},
            { Contracts.Enums.FileType.PDF, () => new InputStream(filePath, blockSize)},
            { Contracts.Enums.FileType.Bzip2, () => new InputStream(filePath, blockSize, fileStream => new BZip2InputStream(fileStream))},
            { Contracts.Enums.FileType.GZip, () => new InputStream(filePath, blockSize, fileStream => new GZipInputStream(fileStream, blockSize))},
            { Contracts.Enums.FileType.Tar, () => new TarArchiveStream(filePath, blockSize)},
            { Contracts.Enums.FileType.Zip, () => new ZipArchiveStream(filePath, blockSize, !acceptEncryptedZipEntries)}
        };

        internal IEnumerable<long> GetOverlaps(long streamLength)
            => Enumerable.Range(1, Math.Max(0, (int)Math.Ceiling((double)streamLength / blockSize) - 1))
                .Select(index => (long)index * blockSize - blockSize / 2);

        internal bool IsArchive
            => new[] { Contracts.Enums.FileType.Bzip2, Contracts.Enums.FileType.GZip, Contracts.Enums.FileType.Tar, Contracts.Enums.FileType.Zip }
            .Contains(ConvertFileType(fileSignature.FileType));

        internal bool ExceedsMaxFileSize(long? maxSize)
            => maxSize.HasValue && maxSize < fileInfo.Length;

        internal bool ExceedsMaxArchiveSize(long? maxArchiveSize)
            => IsArchive && maxArchiveSize.HasValue && maxArchiveSize < fileInfo.Length;

        internal bool TryScanArchiveOrBinary(bool scanOverlaps, out ScanResult lastResult, out ScanResult breakingResult)
            => TryScan(ConvertFileType(fileSignature.FileType), scanOverlaps, out lastResult, out breakingResult);

        internal bool TryScanBinary(bool scanOverlaps, out ScanResult lastResult, out ScanResult breakingResult)
            => TryScan(Contracts.Enums.FileType.Unknown, scanOverlaps, out lastResult, out breakingResult);

        bool TryScan(Contracts.Enums.FileType fileType, bool scanOverlaps, out ScanResult lastResult, out ScanResult breakingResult)
        {
            ScanResult anyResult = null;

            try
            {
                breakingResult = ScanStream(fileType, scanOverlaps)
                        .FirstOrDefault(segmentResult => ResultBuilder.IsBreakResult((anyResult = segmentResult).Result));
            }
            catch(AMSIRejectedByPolicyException e)
            {
                breakingResult = new ScanResult()
                {
                    IsSafe = false,
                    Result = DetectionResult.FileRejected,
                    ResultDetail = e.Message
                };
            }
            catch (Exception e)
            {
                breakingResult = new ScanResult()
                {
                    IsSafe = false,
                    Result = DetectionResult.ApplicationError,
                    ResultDetail = e.ToString()
                };
            }

            if (anyResult != null)
            {
                anyResult.ContentInfo.ContentByteSize = streamLength;
                anyResult.ContentInfo.ContentHash = md5Hash.GetAwaiter().GetResult();
                anyResult.ContentInfo.ContentName = filePath;
                anyResult.ContentInfo.ContentType = ContentType.File;
                anyResult.ContentInfo.ContentFileType = ConvertFileType(fileSignature.FileType);
            }

            lastResult = anyResult;
            return breakingResult == null;
        }

        Contracts.Enums.FileType ConvertFileType(SignatureType fileType)
            => Enum.TryParse<Contracts.Enums.FileType>(fileType.ToString(), true, out var result)
            ? result
            : Contracts.Enums.FileType.Unknown;

        long streamLength = 0;

        IEnumerable<ScanResult> ScanStream(Contracts.Enums.FileType fileType, bool scanOverlaps)
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

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}

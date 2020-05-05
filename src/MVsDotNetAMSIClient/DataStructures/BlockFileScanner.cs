using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using MVsDotNetAMSIClient.Contracts;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class BlockFileScanner : BlockStopwatch
    {
        readonly AMSIClient client;
        readonly BlockFileScannerContext context;

        internal BlockFileScanner(AMSIClient client, string filePath, int blockSize, bool scanOverlaps, bool inspectZipFiles)
        {
            this.client = client;
            context = new BlockFileScannerContext(filePath, blockSize, scanOverlaps, inspectZipFiles);
        }

        Dictionary<FileType, Func<IEnumerable<ScanResult>>> FileProcessors => new Dictionary<FileType, Func<IEnumerable<ScanResult>>>()
            {
                { FileType.Unknown, ScanSeekableStream },
                { FileType.Bzip2, ScanSeekableStream},
                { FileType.GZip, ScanSeekableStream},
                { FileType.Tar, ScanTarArchive},
                { FileType.Zip, ScanZipArchive},
            };

        internal ScanResult Scan()
        {
            ScanResult lastResult = null;

            var breakResult = FileProcessors[context.FileSignature.FileType]()
                .FirstOrDefault(segmentResult => ResultBuilder.IsBreakResult((lastResult = segmentResult).Result));

            var isZip = context.FileSignature.FileType != FileType.Unknown;
            lastResult.ContentHash = context.MD5Hash.GetAwaiter().GetResult();
            lastResult.ContentByteSize = streamLength;
            lastResult.ContentName = context.FilePath;
            lastResult.ContentType = isZip ? ContentType.ZipFile : ContentType.File;
            lastResult.ElapsedTime = Elapsed;
            return breakResult ?? lastResult;
        }

        long streamLength = 0;

        IEnumerable<ScanResult> ScanZipArchive()
        {
            streamLength = 0;
            var zipStream = (ZipInputStream)context.ReadStream;
            while (zipStream.GetNextEntry() is ZipEntry zipEntry)
            {
                int read;
                do
                {
                    streamLength += read = context.ReadStream.Read(context.Buffer, 0, context.BlockSize);
                    var scanResult = client.ScanBuffer(
                        context.Buffer
                        , (uint)context.BlockSize
                        , $"{context.FilePath}@{context.FileStream.Position}>{zipEntry.Name}");

                    yield return scanResult;

                } while (read > 0);
            }
        }

        IEnumerable<ScanResult> ScanTarArchive()
        {
            streamLength = 0;
            var zipStream = (TarInputStream)context.ReadStream;
            while (zipStream.GetNextEntry() is TarEntry tarEntry)
            {
                int read;
                do
                {
                    streamLength += read = context.ReadStream.Read(context.Buffer, 0, context.BlockSize);
                    var scanResult = client.ScanBuffer(
                        context.Buffer
                        , (uint)context.BlockSize
                        , $"{context.FilePath}@{context.FileStream.Position}>{tarEntry.Name}");

                    yield return scanResult;

                } while (read > 0);
            }
        }

        IEnumerable<ScanResult> ScanSeekableStream()
        {
            streamLength = 0;
            int read;
            do
            {
                streamLength += read = context.ReadStream.Read(context.Buffer, 0, context.BlockSize);

                var scanResult = client.ScanBuffer(
                    context.Buffer
                    , (uint)context.BlockSize
                    , $"{context.FilePath}@{context.FileStream.Position}");

                yield return scanResult;

            } while (read > 0);

            if (context.ScanOverlaps)
                foreach (var overlap in context.GetOverlaps(streamLength))
                    yield return ScanSegment(overlap);
        }

        ScanResult ScanSegment(long offset)
        {
            if (context.ReadStream.CanSeek)
                context.ReadStream.Seek(offset, SeekOrigin.Begin);
            else
                context.ScanToPosition(offset);

            context.ReadStream.Read(context.Buffer, 0, context.BlockSize);
            return client.ScanBuffer(context.Buffer, (uint)context.BlockSize, $"{context.FilePath}@{offset}");
        }

        public new void Dispose()
        {
            base.Dispose();
            context?.Dispose();
        }
    }
}

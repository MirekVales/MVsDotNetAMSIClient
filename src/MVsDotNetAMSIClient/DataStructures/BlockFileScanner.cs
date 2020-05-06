using System.Linq;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.DataStructures.Streams;
using System;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class BlockFileScanner : BlockStopwatch
    {
        readonly AMSIClient client;
        readonly BlockFileScannerContext context;

        internal BlockFileScanner(
            AMSIClient client
            , string filePath
            , int blockSize
            , bool scanOverlaps
            , bool inspectZipFiles
            , long? maxArchiveSize
            , bool acceptEncryptedArchive)
        {
            this.client = client;
            context = new BlockFileScannerContext(filePath, blockSize, scanOverlaps, inspectZipFiles, maxArchiveSize, acceptEncryptedArchive);
        }

        internal ScanResult Scan()
        {
            ScanResult breakResult = null;
            ScanResult lastResult = null;

            if (!context.IsArchive || (context.IsArchive && !context.ExceedsMaxArchiveSize))
            {
                try
                {
                    breakResult = ScanStream(context.FileSignature.FileType)
                        .FirstOrDefault(segmentResult => ResultBuilder.IsBreakResult((lastResult = segmentResult).Result));
                }
                catch (Exception e)
                {
                    breakResult = new ScanResult() { Result = DetectionResult.ApplicationError, ResultDetail = e.ToString() };
                }
            }

            if (breakResult == null && context.IsArchive)
            {
                breakResult = ScanStream(FileType.Unknown)
                    .FirstOrDefault(segmentResult => ResultBuilder.IsBreakResult((lastResult = segmentResult).Result));
            }

            UpdateScanResult(lastResult);
            
            return breakResult ?? lastResult;
        }

        void UpdateScanResult(ScanResult lastResult)
        {
            if (lastResult == null)
                return;

            lastResult.ContentHash = context.MD5Hash.GetAwaiter().GetResult();
            lastResult.ContentByteSize = streamLength;
            lastResult.ContentName = context.FilePath;
            lastResult.ContentType = context.IsArchive ? ContentType.ZipFile : ContentType.File;
            lastResult.ElapsedTime = Elapsed;
        }

        long streamLength = 0;

        IEnumerable<ScanResult> ScanStream(FileType fileType)
        {
            streamLength = 0L;
            using (var stream = context.InitiateStream(fileType))
            {
                int read;
                do
                {
                    read = stream.Read(context.Buffer, context.BlockSize);
                    var scanResult = client.ScanBuffer(
                        context.Buffer
                        , (uint)context.BlockSize
                        , $"{context.FilePath}@{stream.Position - context.BlockSize}>{stream.CurrentEntryName}");

                    yield return scanResult;

                } while (read > 0);
                streamLength = stream.Position;
            }

            if (context.ScanOverlaps)
                using (var stream = context.InitiateStream(fileType))
                    foreach (var overlap in context.GetOverlaps(streamLength))
                        yield return ScanSegment(stream, overlap);
        }

        ScanResult ScanSegment(IInputStream stream, long offset)
        {
            stream.ScanTo(context.Buffer, offset);
            stream.Read(context.Buffer, context.BlockSize);
            return client.ScanBuffer(context.Buffer, (uint)context.BlockSize, $"{context.FilePath}@{offset}");
        }

        public new void Dispose()
            => base.Dispose();
    }
}

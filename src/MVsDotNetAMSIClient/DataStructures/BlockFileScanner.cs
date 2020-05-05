using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class BlockFileScanner : BlockStopwatch
    {
        readonly AMSIClient client;
        readonly byte[] buffer;
        readonly string path;
        readonly string md5Hash;
        readonly FileStream fileStream;
        readonly long fileLength;
        readonly int blockSize;
        readonly long[] overlapsOffsets;

        internal BlockFileScanner(AMSIClient client, string path, int blockSize, bool scanOverlaps)
        {
            this.client = client;
            buffer = new byte[blockSize];
            this.path = path;
            md5Hash = new FileInfo(path).GetFileMD5Hash();
            fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, blockSize, FileOptions.RandomAccess);
            fileStream.Lock(0, fileStream.Length);
            fileLength = fileStream.Length;
            this.blockSize = blockSize;
            overlapsOffsets = scanOverlaps ? GetOverlaps().ToArray() : new long [0];
        }

        IEnumerable<long> GetOverlaps()
            => Enumerable.Range(1, Math.Max(0, (int)Math.Ceiling((double)fileLength / blockSize) - 1))
                .Select(index => (long)index * blockSize - blockSize / 2);

        internal ScanResult Scan()
        {
            var breakResults = new[] { 
                DetectionResult.ApplicationError
                , DetectionResult.BlockedByAdministrator
                , DetectionResult.IdentifiedAsMalware };
            ScanResult lastResult = null;
            
            var breakResult = ScanFile().FirstOrDefault(segmentResult => breakResults.Contains((lastResult = segmentResult).Result));
            lastResult.ContentHash = md5Hash;
            lastResult.ContentByteSize = fileLength;
            lastResult.ContentName = path;
            lastResult.ContentType = ContentType.File;
            lastResult.ElapsedTime = Elapsed;
            return breakResult ?? lastResult;
        }

        IEnumerable<ScanResult> ScanFile()
        {
            int read;
            do
            {
                read = fileStream.Read(buffer, 0, blockSize);
                var scanResult = client.ScanBuffer(buffer, (uint)blockSize, $"{path}@{fileStream.Position}");

                yield return scanResult;

            } while (read > 0);

            foreach (var overlap in overlapsOffsets)
                yield return ScanSegment(overlap);
        }

        ScanResult ScanSegment(long offset)
        {
            fileStream.Seek(offset, SeekOrigin.Begin);
            fileStream.Read(buffer, 0, blockSize);
            return client.ScanBuffer(buffer, (uint)blockSize, $"{path}@{offset}");
        }

        public void Dispose()
        {
            fileStream?.Unlock(0, fileStream.Length);
            fileStream?.Dispose();
        }
    }
}

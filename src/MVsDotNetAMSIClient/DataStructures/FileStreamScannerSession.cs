using MVsDotNetAMSIClient.Contracts;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class FileStreamScannerSession : BlockStopwatch
    {
        readonly FileStreamScanner streamScanner;
        readonly AMSIClientConfiguration configuration;

        internal FileStreamScannerSession(
            AMSIClient client
            , string filePath
            , int blockSize
            , bool acceptEncryptedArchive)
        {
            streamScanner = new FileStreamScanner(
                client
                , filePath
                , blockSize
                , acceptEncryptedArchive);
            configuration = client.Configuration;
        }

        internal ScanResult Scan()
        {
            if (streamScanner.ExceedsMaxFileSize(configuration.FileScannerSkipFilesLargerThan))
                return streamScanner.GetRejectedResult(
                    $"File exceeded max allowed size {configuration.FileScannerSkipFilesLargerThan.BytesToString()}");

            ScanResult lastResult = null;
            ScanResult breakingResult = null;

            var shouldScanAsArchive = streamScanner.IsArchive 
                && !configuration.FileScannerSkipZipFileInspection
                && !streamScanner.ExceedsMaxArchiveSize(configuration.FileScannerSkipZipFileInspectionForFilesLargerThan);

            if ((!streamScanner.IsArchive || configuration.FileScannerSkipZipFileInspection)
                || (shouldScanAsArchive
                && streamScanner.TryScanArchiveOrBinary(!configuration.FileScannerSkipOverlapsScan, out lastResult, out breakingResult)))
            {
                streamScanner.TryScanBinary(!configuration.FileScannerSkipOverlapsScan, out lastResult, out breakingResult);
            }

            if (lastResult != null)
                lastResult.DetectionResultInfo.ElapsedTime = Elapsed;
            
            return breakingResult ?? lastResult;
        }

        public new void Dispose()
            => base.Dispose();
    }
}

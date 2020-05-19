using System;
using System.ComponentModel;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.NativeMethods;
using MVsDotNetAMSIClient.DataStructures;
using MVsDotNetAMSIClient.DetailProviders;
using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient
{
    public class AMSISession : IDisposable
    {
        readonly AMSIClient client;
        readonly AMSIHandleSession sessionHandle;

        internal AMSISession(AMSIClient client)
        {
            this.client = client;
            var result = AMSIMethods.AmsiOpenSession(client.ContextHandle, out sessionHandle);
            result.CheckResult(nameof(AMSIMethods.AmsiOpenSession));
            sessionHandle.Context = client.ContextHandle;
            sessionHandle.CheckHandle();
        }

        public void Dispose()
            => sessionHandle?.Dispose();

        public ScanResult ScanString(string content, string contentName)
        {
            using (var resultBuilder = new ResultBuilder(
                new ScanContext(
                client
                , sessionHandle
                , contentName
                , ContentType.String
                , FileType.Unknown
                , content.Length * 4
                , content.GetMD5Hash())))
            {
                var result = AMSIMethods.AmsiScanString(
                    client.ContextHandle
                    , content
                    , contentName
                    , sessionHandle
                    , out var resultNumber);
                ScanResult scanResult = null;
                result.CheckResult(
                    success: _ => scanResult = resultBuilder.ToResult(resultNumber)
                    , failure: _ => scanResult = resultBuilder.ToResult(new Win32Exception(result)));
                return scanResult;
            }
        }

        public ScanResult ScanBuffer(byte[] buffer, int length, string contentName)
            => ScanBuffer(buffer, (uint)length, contentName);

        public ScanResult ScanBuffer(byte[] buffer, uint length, string contentName)
        {
            using (var resultBuilder = new ResultBuilder(
                new ScanContext(
                client
                , sessionHandle
                , contentName
                , ContentType.ByteArray
                , FileType.Unknown
                , buffer.LongLength
                , buffer.GetMD5Hash())))
            {
                var result = AMSIMethods.AmsiScanBuffer(
                    client.ContextHandle
                    , buffer
                    , length
                    , contentName
                    , sessionHandle
                    , out var resultNumber);
                ScanResult scanResult = null;
                result.CheckResult(
                    success: _ => scanResult = resultBuilder.ToResult(resultNumber)
                    , failure: _ => scanResult = resultBuilder.ToResult(new Win32Exception(result)));
                return scanResult;
            }
        }

        public ScanResult ScanFile(string filePath)
        {
            client.DetermineDetectionEngine();

            using (var resultBuilder = new ResultBuilder(
                new ScanContext(client, null, filePath, ContentType.File, FileType.Unknown, 0, null)))
            using (var signatureReader = new FileSignatureReader(filePath))
                if (!signatureReader.FileExists())
                    return resultBuilder.ToResult(DetectionResult.FileNotExists, $"File not found at {filePath}");
                else if (signatureReader.IsFileBlocked())
                    return resultBuilder.ToResultBlocked();

            using (var reader = new FileStreamScannerSession(
                client
                , filePath
                , client.Configuration.FileScannerBlockSize
                , client.Configuration.FileScannerAcceptZipFileWithEncryptedEntry))
                return reader.Scan();
        }
    }
}

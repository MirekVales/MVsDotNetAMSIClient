using System;
using System.Diagnostics;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.NativeMethods;
using MVsDotNetAMSIClient.DataStructures;
using MVsDotNetAMSIClient.DetailProviders;

namespace MVsDotNetAMSIClient
{
    public class AMSIClient : IDisposable
    {
        public static AMSIClient Create()
            => Create(AMSIClientConfiguration.Default);

        public static AMSIClient Create(AMSIClientConfiguration configuration)
        {
            if (IsAvailable())
                return new AMSIClient(configuration);

            throw AMSIException.AMSINotFound;
        }

        public static bool IsAvailable()
            => AMSIMethods.IsDllImportPossible();

        public int ProcessId { get; }
        public string Name { get; }
        public AMSIClientConfiguration Configuration { get; }
        public DetectionEngine DetectionEngine { get; private set; }

        internal readonly AMSIHandleContext ContextHandle;

        AMSIClient(AMSIClientConfiguration configuration)
        {
            Configuration = configuration;
            DetectionEngine = configuration.DetectionEngine;

            var result = AMSIMethods.AmsiInitialize(
                Name = $"{AppDomain.CurrentDomain.FriendlyName} ({ProcessId = Process.GetCurrentProcess().Id})", out ContextHandle);
            result.CheckResult(nameof(AMSIMethods.AmsiInitialize));
            ContextHandle.CheckHandle();
        }

        AMSISession CreateSession()
        {
            DetermineDetectionEngine();

            if (DetectionEngine == DetectionEngine.Unknown && Configuration.SkipAMSIIfNoDetectionEngineFound)
                throw AMSIException.NoDetectionEngineFound;

            return new AMSISession(this);
        }

        void DetermineDetectionEngine()
        {
            DetectionEngine = Configuration.DetectionEngine == DetectionEngine.Unknown
                ? AVEngineDetector.DetectEngine().EngineType
                : DetectionEngine;
        }

        public ScanResult ScanString(string content, string contentName)
        {
            using (var session = CreateSession())
                return session.ScanString(content, contentName);
        }

        public ScanResult ScanBuffer(byte[] buffer, uint length, string contentName)
        {
            using (var session = CreateSession())
                return session.ScanBuffer(buffer, length, contentName);
        }

        public ScanResult ScanFile(string filePath)
        {
            DetermineDetectionEngine();

            using (var resultBuilder = new ResultBuilder(
                new ScanContext(this, null, filePath, ContentType.File, FileType.Unknown, 0, null)))
                if (new FileSignatureReader().IsFileBlocked(filePath))
                    return resultBuilder.ToResultBlocked();

            using (var reader = new FileStreamScannerSession(
                this
                , filePath
                , Configuration.FileScannerBlockSize
                , Configuration.FileScannerAcceptZipFileWithEncryptedEntry))
                return reader.Scan();
        }

        public ScanResult TestEICARString()
            => ScanString(EICARTestData.EICARText, nameof(EICARTestData.EICARText));

        public ScanResult TestEICARByteArray()
            => ScanBuffer(EICARTestData.EICARZippedBytes, (uint)EICARTestData.EICARZippedBytes.Length, nameof(EICARTestData.EICARZippedBytes));

        public IEnumerable<DetectionEngineInfo> ListDetectionEngines()
            => AVEngineDetector.ListDetectionEngines();

        public void Dispose()
            => ContextHandle?.Dispose();
    }
}

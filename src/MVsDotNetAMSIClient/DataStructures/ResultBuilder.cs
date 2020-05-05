using System;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.NativeMethods;
using MVsDotNetAMSIClient.DetailProviders;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class ResultBuilder : BlockStopwatch
    {
        internal ScanContext ScanContext { get; }

        internal ResultBuilder(ScanContext scanContext)
            => ScanContext = scanContext;

        internal ScanResult ToResult(int detectionResultNumber)
        {
            var result = GetDetectionResult(detectionResultNumber);
            var detail = ScanContext.Client.Configuration.SkipScanResultDetailRetrieval || !IsBreakResult(result)
                ? null
                : DetailProviderFactory
                    .GetDetailProvider(ScanContext.Client.DetectionEngine)?.GetDetail(this);

            return new ScanResult()
            {
                TimeStamp = Start,

                ContentName = ScanContext.ContentName,
                ContentType = ScanContext.ContentType,
                ContentByteSize = ScanContext.Size,
                ContentHash = ScanContext.Hash,

                MalwareId = detectionResultNumber,
                ThreatLevel = Math.Min(1, (float)Math.Round(detectionResultNumber / 32768d, 2)),
                Result = result,
                ResultDetail = null,

                ScanProcessAppName = ScanContext.Client.Name,
                ScanEnvironmentMachineName = Environment.MachineName,
                ScanUsername = Environment.UserDomainName,
                DetectionEngine = ScanContext.Client.DetectionEngine,
                ElapsedTime = Elapsed,

                EngineResultDetail = detail
            };
        }

        internal ScanResult ToResult(Exception exception)
            => new ScanResult()
            {
                TimeStamp = Start,

                ContentName = ScanContext.ContentName,
                ContentType = ScanContext.ContentType,
                ContentByteSize = ScanContext.Size,
                ContentHash = ScanContext.Hash,

                MalwareId = null,
                ThreatLevel = null,
                Result = DetectionResult.ApplicationError,
                ResultDetail = exception.ToString(),

                ScanProcessAppName = ScanContext.Client.Name,
                ScanEnvironmentMachineName = Environment.MachineName,
                ScanUsername = Environment.UserDomainName,
                DetectionEngine = ScanContext.Client.DetectionEngine,
                ElapsedTime = Elapsed
            };

        DetectionResult GetDetectionResult(int result)
        {
            if (result < 0)
                return DetectionResult.ApplicationError;
            if (result == (int)AMSIResult.AMSI_RESULT_CLEAN)
                return DetectionResult.Clean;
            if (result == (int)AMSIResult.AMSI_RESULT_NOT_DETECTED)
                return DetectionResult.NotDetected;
            if (result >= (int)AMSIResult.AMSI_RESULT_DETECTED)
                return DetectionResult.IdentifiedAsMalware;

            return DetectionResult.BlockedByAdministrator;
        }

        internal static HashSet<DetectionResult> BreakResults = new HashSet<DetectionResult>(new [] {
                DetectionResult.ApplicationError
                , DetectionResult.BlockedByAdministrator
                , DetectionResult.IdentifiedAsMalware });

        internal static bool IsBreakResult(DetectionResult result)
            => BreakResults.Contains(result);

    }
}
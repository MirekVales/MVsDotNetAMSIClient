using System;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts;
using System.Runtime.InteropServices;
using MVsDotNetAMSIClient.NativeMethods;
using MVsDotNetAMSIClient.DetailProviders;
using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class ResultBuilder : BlockStopwatch
    {
        internal ScanContext ScanContext { get; }

        internal ResultBuilder(ScanContext scanContext)
            => ScanContext = scanContext;

        internal ScanResult ToResultBlocked()
            => ToResult(-1);

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
                ContentInfo = CreateContentInfo(),
                DetectionResultInfo = new ScanResultDetectionInfo()
                {
                    MalwareID = GetMalwareID(detectionResultNumber),
                    ThreatLevel = Math.Min(1, (float)Math.Round(detectionResultNumber / 32768d, 2)),
                    EngineResultDetail = detail,
                    ElapsedTime = Elapsed,
                },
                Result = result,
                ResultDetail = null,
                DetectionEngineInfo = CreateDetectionEngineInfo()
            };
        }

        internal ScanResult ToResult(DetectionResult result, string reason)
            => new ScanResult()
            {
                TimeStamp = Start,
                ContentInfo = CreateContentInfo(),
                Result = result,
                ResultDetail = reason,
                DetectionResultInfo = new ScanResultDetectionInfo()
                {
                    MalwareID = null,
                    ThreatLevel = null,
                    EngineResultDetail = null,
                    ElapsedTime = Elapsed,
                },
                DetectionEngineInfo = CreateDetectionEngineInfo()
            };

        internal ScanResult ToResult(Exception exception)
            => new ScanResult()
            {
                TimeStamp = Start,
                ContentInfo = CreateContentInfo(),
                Result = DetectionResult.ApplicationError,
                ResultDetail = exception.ToString(),
                DetectionResultInfo = new ScanResultDetectionInfo()
                {
                    MalwareID = null,
                    ThreatLevel = null,
                    EngineResultDetail = null,
                    ElapsedTime = Elapsed,
                },
                DetectionEngineInfo = CreateDetectionEngineInfo()
            };

        DetectionResult GetDetectionResult(int result)
        {
            if (result < 0)
                return DetectionResult.FileBlocked;
            if (result == (int)AMSIResult.AMSI_RESULT_CLEAN)
                return DetectionResult.Clean;
            if (result == (int)AMSIResult.AMSI_RESULT_NOT_DETECTED)
                return DetectionResult.NotDetected;
            if (result >= (int)AMSIResult.AMSI_RESULT_DETECTED)
                return DetectionResult.IdentifiedAsMalware;

            return DetectionResult.BlockedByAdministrator;
        }

        int? GetMalwareID(int detectionResultID)
            => detectionResultID > (int)AMSIResult.AMSI_RESULT_NOT_DETECTED ? detectionResultID : (int?)null;

        internal static HashSet<DetectionResult> BreakResults = new HashSet<DetectionResult>(new[] {
                DetectionResult.ApplicationError
                , DetectionResult.BlockedByAdministrator
                , DetectionResult.IdentifiedAsMalware
                , DetectionResult.FileBlocked });

        internal static bool IsBreakResult(DetectionResult result)
            => BreakResults.Contains(result);

        ScanResultContentInfo CreateContentInfo()
           => new ScanResultContentInfo()
           {
               ContentName = ScanContext.ContentName,
               ContentType = ScanContext.ContentType,
               ContentByteSize = ScanContext.Size,
               ContentFileType = ScanContext.ContentFileType,
               ContentHash = ScanContext.Hash,
           };

        ScanResultEngineInfo CreateDetectionEngineInfo()
            => new ScanResultEngineInfo()
            {
                ClientProcessAppName = ScanContext.Client.Name,
                ClientProcessUsername = Environment.UserDomainName,
                EnvironmentMachineName = Environment.MachineName,
                EnvironmentOSDescription = RuntimeInformation.OSDescription,
                DetectionEngine = ScanContext.Client.DetectionEngine,
            };
    }
}
﻿using System;
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

                ContentName = ScanContext.ContentName,
                ContentType = ScanContext.ContentType,
                ContentByteSize = ScanContext.Size,
                ContentFileType = ScanContext.ContentFileType,
                ContentHash = ScanContext.Hash,

                MalwareID = GetMalwareID(detectionResultNumber),
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

        internal ScanResult ToResultRejected(string reason)
            => new ScanResult()
            {
                TimeStamp = Start,

                ContentName = ScanContext.ContentName,
                ContentType = ScanContext.ContentType,
                ContentByteSize = ScanContext.Size,
                ContentFileType = ScanContext.ContentFileType,
                ContentHash = ScanContext.Hash,

                MalwareID = null,
                ThreatLevel = null,
                Result = DetectionResult.Rejected,
                ResultDetail = reason,

                ScanProcessAppName = ScanContext.Client.Name,
                ScanEnvironmentMachineName = Environment.MachineName,
                ScanUsername = Environment.UserDomainName,
                DetectionEngine = ScanContext.Client.DetectionEngine,
                ElapsedTime = Elapsed,

                EngineResultDetail = null
            };

        internal ScanResult ToResult(Exception exception)
            => new ScanResult()
            {
                TimeStamp = Start,

                ContentName = ScanContext.ContentName,
                ContentType = ScanContext.ContentType,
                ContentByteSize = ScanContext.Size,
                ContentFileType = ScanContext.ContentFileType,
                ContentHash = ScanContext.Hash,

                MalwareID = null,
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

        internal static HashSet<DetectionResult> BreakResults = new HashSet<DetectionResult>(new [] {
                DetectionResult.ApplicationError
                , DetectionResult.BlockedByAdministrator
                , DetectionResult.IdentifiedAsMalware
                , DetectionResult.FileBlocked });

        internal static bool IsBreakResult(DetectionResult result)
            => BreakResults.Contains(result);
    }
}
using System;
using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient.Contracts
{
    public class ScanResult
    {
        /// <summary>
        /// Date and time when scan was started
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Details about content provided as input
        /// </summary>
        public ScanResultContentInfo ContentInfo { get; set; }

        /// <summary>
        /// Details about detection engine involved in scan
        /// </summary>
        public ScanResultEngineInfo DetectionEngineInfo { get; set; }

        /// <summary>
        /// Aggregated result of detection
        /// True indicates no signs of malware
        /// False indicates a positive result, rejection or application error
        /// </summary>
        public bool IsSafe { get; set; }

        /// <summary>
        /// Overall result of detection
        /// </summary>
        public DetectionResult Result { get; set; }

        /// <summary>
        /// Overall extra information about detection result
        /// </summary>
        public string ResultDetail { get; set; }

        /// <summary>
        /// Details about detection result
        /// </summary>
        public ScanResultDetectionInfo DetectionResultInfo { get; set; }

    }
}

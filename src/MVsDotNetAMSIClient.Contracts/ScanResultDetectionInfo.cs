using System;

namespace MVsDotNetAMSIClient.Contracts
{
    public class ScanResultDetectionInfo
    {
        /// <summary>
        /// ID or family id of malware detected
        /// </summary>
        public int? MalwareID { get; set; }
        /// <summary>
        /// Normalized level of threat (0 - no threat, 1 max threat)
        /// </summary>
        public float? ThreatLevel { get; set; }
        /// <summary>
        /// In case of detected malware and supported detection engine,
        /// this detail contains additional information about malware
        /// </summary>
        public IScanResultDetail EngineResultDetail { get; set; }
        /// <summary>
        /// Time spent on scan action
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }
    }
}

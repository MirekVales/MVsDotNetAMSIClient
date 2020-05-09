using System;

namespace MVsDotNetAMSIClient.Contracts
{
    public class ScanResult
    {
        /// <summary>
        /// Date and time when scan was started
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Name of scanned content provided by initiator
        /// </summary>
        public string ContentName { get; set; }
        /// <summary>
        /// Byte size of scanned content
        /// </summary>
        public long ContentByteSize { get; set; }
        /// <summary>
        /// Type of scanned content
        /// </summary>
        public ContentType ContentType { get; set; }
        /// <summary>
        /// Type of scanned content according to detected file signature
        /// </summary>
        public FileType ContentFileType { get; set; }
        /// <summary>
        /// MD5 hash of scanned content
        /// </summary>
        public string ContentHash { get; set; }

        /// <summary>
        /// Overall result of scanning action
        /// </summary>
        public DetectionResult Result { get; set; }
        /// <summary>
        /// Id or family id of malware detected
        /// </summary>
        public int? MalwareID { get; set; }
        /// <summary>
        /// Normalized level of threat (0 - no threat, 1 max threat)
        /// </summary>
        public float? ThreatLevel { get; set; }
        /// <summary>
        /// Extra information about scanning action
        /// </summary>
        public string ResultDetail { get; set; }

        /// <summary>
        /// Name of AMSI client application 
        /// </summary>
        public string ScanProcessAppName { get; set; }
        /// <summary>
        /// AV/malware engine invoked over AMSI
        /// </summary>
        public DetectionEngine DetectionEngine { get; set; }
        /// <summary>
        /// Machine name of environment where scanning action is executed
        /// </summary>
        public string ScanEnvironmentMachineName { get; set; }
        /// <summary>
        /// Name of user that executed scanning action
        /// </summary>
        public string ScanUsername { get; set; }
        /// <summary>
        /// Time spent on scanning action
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }
        
        /// <summary>
        /// In case of detected malware and supported detection engine,
        /// this detail contains additional information about malware
        /// </summary>
        public IScanResultDetail EngineResultDetail { get; set; }
    }
}

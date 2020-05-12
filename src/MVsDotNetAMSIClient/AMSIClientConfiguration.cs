using System;
using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient
{
    public class AMSIClientConfiguration
    {
        /// <summary>
        /// Primary AV/malware engine that is known to be invoked by AMSI 
        /// If default value Unknown provided, engine will be detected automatically by client
        /// Specify this value only if you positively know installed primary AV engine
        /// </summary>
        public DetectionEngine DetectionEngine { get; set; }

        /// <summary>
        /// If there are no active active AV/malware engines (according to system registry
        /// or according to information provided in <see cref="DetectionEngine"/>),
        /// client throws exception before performing actual AMSI call
        /// </summary>
        public bool SkipAMSIIfNoDetectionEngineFound { get; set; }

        /// <summary>
        /// If set to true, client does not fetch scan result detail
        /// Detection detail retrieval may increase time needed to finish scan
        /// Detail retrieval is supported only for some AV engines (e.g. Windows Defender)
        /// </summary>
        public bool SkipScanResultDetailRetrieval { get; set; }

        /// <summary>
        /// Maximum time allocated to retrieve detection detail
        /// If exceeded, retrieval is aborted and no detection detail is returned
        /// </summary>
        public TimeSpan MaximumScanResultRetrievalTime { get; set; } = TimeSpan.FromSeconds(2.5);

        /// <summary>
        /// If file is to be scanned, it is first split into byte array chunks that are sent to AMSI separately
        /// Default size of byte array chunks is 10 MB
        /// To improve detection efficiency by avoiding signature cutting, overlap scan feature can be enabled
        /// </summary>
        public int FileScannerBlockSize { get; set; } = 10 * 1000 * 1000;

        /// <summary>
        /// File scanner tries to avoid infective scanning of segmented buffer chunks by performing
        /// additional scan of segment that is created by taking last half of previous chunk
        /// and first half of next chunk. This costs additional time but increases detection effectivity
        /// as some detectable signatures in file may have been splitted unintentionally by chunking
        /// </summary>
        public bool FileScannerSkipOverlapsScan { get; set; }

        /// <summary>
        /// If set, file scanner rejects to scan file exceeding this size threshold in bytes
        /// </summary>
        public long? FileScannerSkipFilesLargerThan { get; set; }

        /// <summary>
        /// File scanner tries to analyze entries compressed in ZIP archive
        /// Only first level entries are analyzed
        /// </summary>
        public bool FileScannerSkipZipFileInspection { get; set; }

        /// <summary>
        /// If set, file scanner does not execute zip inspection for archive exceeding this threshold in bytes
        /// </summary>
        public long? FileScannerSkipZipFileInspectionForFilesLargerThan { get; set; }

        /// <summary>
        /// If set, file scanner also accepts zip archives with encrypted entries
        /// </summary>
        public bool FileScannerAcceptZipFileWithEncryptedEntry { get; set; }

        public static AMSIClientConfiguration Default
            => new AMSIClientConfiguration();
    }
}

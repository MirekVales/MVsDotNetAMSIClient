using System;
using MVsDotNetAMSIClient.Contracts;

namespace MVsDotNetAMSIClient
{
    public class AMSIClientConfiguration
    {
        /// <summary>
        /// Primary AV/malware engine that is known to be invoked by AMSI
        /// If Unknown is provided, engine will be detected automatically by client
        /// Specify this value only if you positively know primary AV engine
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
        /// </summary>
        public bool SkipScanResultDetailRetrieval { get; set; }

        /// <summary>
        /// Maximum time allocated to retrieve detection detail
        /// If exceeded, no detection detail is returned
        /// </summary>
        public TimeSpan MaximumScanResultRetrievalTime { get; set; } = TimeSpan.FromSeconds(2.5);

        public static AMSIClientConfiguration Default
            => new AMSIClientConfiguration();
    }
}

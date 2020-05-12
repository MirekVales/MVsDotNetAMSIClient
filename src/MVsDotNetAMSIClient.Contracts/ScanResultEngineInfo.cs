using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient.Contracts
{
    public class ScanResultEngineInfo
    {
        /// <summary>
        /// Name of AMSI client application 
        /// </summary>
        public string ClientProcessAppName { get; set; }
        /// <summary>
        /// Name of user that executed scan action
        /// </summary>
        public string ClientProcessUsername { get; set; }
        /// <summary>
        /// AV/malware engine invoked over AMSI
        /// </summary>
        public DetectionEngine DetectionEngine { get; set; }
        /// <summary>
        /// Machine name of environment where scan action is executed
        /// </summary>
        public string EnvironmentMachineName { get; set; }
        /// <summary>
        /// Description of operating system on which client is executed
        /// </summary>
        public string EnvironmentOSDescription { get; set; }
    }
}

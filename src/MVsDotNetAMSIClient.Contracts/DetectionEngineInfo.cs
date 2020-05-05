using System;

namespace MVsDotNetAMSIClient.Contracts
{
    /// <summary>
    /// Information about AV/malware engine installed on computer
    /// Data is taken from system registry
    /// </summary>
    public class DetectionEngineInfo
    {
        public DetectionEngine EngineType { get; set; }
        public Guid InstanceGuid { get; set; }
        public string DisplayName { get; set; }
        public string CompanyName { get; set; }
        public uint ProductState { get; set; }
        public string PathToSignedProductExe { get; set; }
        public string VersionInfo { get; set; }
        public bool IsEnabled { get; set; }

        public static DetectionEngineInfo Empty
            => new DetectionEngineInfo();
    }
}

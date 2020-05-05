namespace MVsDotNetAMSIClient.Contracts
{
    /// <summary>
    /// Additional information provided by WindowsDefender
    /// Taken from Windows event log
    /// </summary>
    public class WindowsDefenderDetail : IScanResultDetail
    {
        public int EventID { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string ProductVersion { get; set; }
        public string ThreatID { get; set; }
        public string ThreatName { get; set; }
        public int SeverityID { get; set; }
        public string SeverityName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string FWLink { get; set; }
        public string SecurityIntelligenceVersion { get; set; }
        public string EngineVersion { get; set; }
    }
}

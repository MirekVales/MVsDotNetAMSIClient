namespace MVsDotNetAMSIClient.Contracts
{
    /// <summary>
    /// Enumeration of recognized AV/malware engines
    /// that should be hyphothetically possible to call over AMSI
    /// </summary>
    public enum DetectionEngine
    {
        Unknown,
        Avast,
        AVG,
        BitDefender,
        BitDefenderFree,
        ESET,
        ESETSecurity,
        GDataAntivirus,
        KasperskyEndpointSecurity,
        KasperskyAntivirus,
        Norton360,
        NortonSecurity,
        SophosHome,
        WindowsDefender,
        Other
    }
}

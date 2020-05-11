namespace MVsDotNetAMSIClient.Contracts.Enums
{
    public enum DetectionResult
    {
        /// <summary>
        /// Result has not been received
        /// </summary>
        Unknown,
        /// <summary>
        /// No threat detected and this probably will not change after definition is updated in future
        /// </summary>
        Clean,
        /// <summary>
        /// No threat detected but this may change once definition is updated in future
        /// </summary>
        NotDetected,
        /// <summary>
        /// Administrator policy blocked this content
        /// </summary>
        BlockedByAdministrator,
        /// <summary>
        /// Threat detected. Content is identified as malware
        /// </summary>
        IdentifiedAsMalware,
        /// <summary>
        /// File access was blocked by AV engine before scan begun
        /// </summary>
        FileBlocked,
        /// <summary>
        /// File was rejected because of MVsDotNetAMSIClient policies
        /// </summary>
        FileRejected,
        /// <summary>
        /// File was not found at given path
        /// </summary>
        FileNotExists,
        /// <summary>
        /// Scan could not be executed. There was failure when calling AMSI, probably related to AV engine
        /// </summary>
        ApplicationError
    }
}

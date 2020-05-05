namespace MVsDotNetAMSIClient.Contracts
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
        /// Scan could not be executed. There was failure when calling AMSI, probably related to AV engine
        /// </summary>
        ApplicationError
    }
}

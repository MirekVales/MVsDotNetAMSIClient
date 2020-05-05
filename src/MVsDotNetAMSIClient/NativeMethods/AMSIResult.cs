namespace MVsDotNetAMSIClient.NativeMethods
{
    internal enum AMSIResult
    {
        AMSI_RESULT_CLEAN = 0,
        AMSI_RESULT_NOT_DETECTED = 1,
        AMSI_RESULT_BLOCKED_BY_ADMIN = 16384, // 16384 -  20479
        AMSI_RESULT_DETECTED = 32768
    }
}
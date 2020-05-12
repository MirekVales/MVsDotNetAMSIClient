using System;

namespace MVsDotNetAMSIClient
{
    public class AMSIRejectedByPolicyException : Exception
    {
        public AMSIRejectedByPolicyException()
        { }

        public AMSIRejectedByPolicyException(string message) : base(message)
        { }

        public AMSIRejectedByPolicyException(string message, Exception innerException) : base(message, innerException)
        { }

        public static AMSIRejectedByPolicyException ZipContainsCryptedEntry(string entryName)
            => new AMSIRejectedByPolicyException($"Zip archive contains encrypted entry {entryName}. AMSI call was stopped by policy");
    }
}

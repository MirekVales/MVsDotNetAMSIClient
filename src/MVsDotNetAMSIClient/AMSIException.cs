using System;
using MVsDotNetAMSIClient.NativeMethods;

namespace MVsDotNetAMSIClient
{
    public class AMSIException : Exception
    {
        public AMSIException()
        { }

        public AMSIException(string message) : base(message)
        { }

        public AMSIException(string message, Exception innerException) : base(message, innerException)
        { }

        public static AMSIException AMSINotFound
            => new AMSIException($"AMSI ({AMSIMethods.AMSIDllName}) was not found on this machine");

        public static AMSIException AMSIFailedToExecute(string methodName, Exception innerException)
            => new AMSIException($"AMSI method {methodName} failed to execute", innerException);

        public static AMSIException AMSIInvalidState
            => new AMSIException("AMSI is in invalid state to perform operation. Check configuration of available AMSI providers");

        public static AMSIException NoDetectionEngineFound
            => new AMSIException("No detection engine found. AMSI call cannot be executed");

        public static AMSIException FailedToInitialize()
            => new AMSIException("AMSI failed to initialize");

        public static AMSIException FailedToInitializeSession()
            => new AMSIException("AMSI failed to initialize session");

        public static AMSIException ZipContainsCryptedEntry(string entryName)
            => new AMSIException($"Zip archive contains encrypted entry {entryName}. AMSI call cannot be executed");
    }
}

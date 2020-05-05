using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.NativeMethods;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class ScanContext
    {
        internal AMSIClient Client { get; }
        internal AMSIHandleSession SessionHandle { get; }
        internal string ContentName { get; }
        internal ContentType ContentType { get; }
        internal long Size { get; }
        internal string Hash { get; }

        internal ScanContext(
            AMSIClient client
            , AMSIHandleSession sessionHandle
            , string contentName
            , ContentType contentType
            , long size
            , string hash)
        {
            Client = client;
            SessionHandle = sessionHandle;
            ContentName = contentName;
            ContentType = contentType;
            Size = size;
            Hash = hash;
        }
    }
}

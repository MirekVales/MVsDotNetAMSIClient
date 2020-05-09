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
        internal FileType ContentFileType { get; }
        internal long Size { get; }
        internal string Hash { get; }

        internal ScanContext(
            AMSIClient client
            , AMSIHandleSession sessionHandle
            , string contentName
            , ContentType contentType
            , FileType contentFileType
            , long size
            , string hash)
        {
            Client = client;
            SessionHandle = sessionHandle;
            ContentName = contentName;
            ContentType = contentType;
            ContentFileType = contentFileType;
            Size = size;
            Hash = hash;
        }
    }
}

using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient.Contracts
{
    public class ScanResultContentInfo
    {
        /// <summary>
        /// Name of scanned content provided by initiator
        /// </summary>
        public string ContentName { get; set; }
        /// <summary>
        /// Byte size of scanned content
        /// </summary>
        public long ContentByteSize { get; set; }
        /// <summary>
        /// Type of scanned content
        /// </summary>
        public ContentType ContentType { get; set; }
        /// <summary>
        /// Type of scanned content according to detected file signature
        /// </summary>
        public FileType ContentFileType { get; set; }
        /// <summary>
        /// MD5 hash of scanned content
        /// </summary>
        public string ContentHash { get; set; }
    }
}

using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient.DetailProviders
{
    internal class FileSignature
    {
        public FileType FileType { get; }
        public string Name { get; }
        public string Bytes { get; }
        public int Length { get; }

        internal FileSignature(FileType fileType, string name, string bytes, int length)
        {
            FileType = fileType;
            Name = name;
            Bytes = bytes;
            Length = length;
        }

        internal static FileSignature Unknown
            => new FileSignature(FileType.Unknown, nameof(FileType.Unknown), "", 0);
    }
}

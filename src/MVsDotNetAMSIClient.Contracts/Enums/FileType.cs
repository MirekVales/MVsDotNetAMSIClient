using System;

namespace MVsDotNetAMSIClient.Contracts.Enums
{
    [Flags]
    public enum FileType
    {
        Unknown = 0,

        Executable = 1,

        Zip = 2,
        Bzip2 = 4,
        GZip = 8,
        Tar = 16,

        CompoundFileBinary = 2048,
        PDF = 4096,
        MicrosoftOfficeDocument = 8192,
    }
}

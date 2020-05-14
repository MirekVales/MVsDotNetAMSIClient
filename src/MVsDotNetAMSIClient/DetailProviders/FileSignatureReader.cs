using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient.DetailProviders
{
    internal class FileSignatureReader : IDisposable
    {
        internal static IEnumerable<FileSignature> Signatures()
        {
            yield return new FileSignature(FileType.CompoundFileBinary, "Compound File Binary", "D0-CF-11-E0-A1-B1-1A-E1", 8);
            yield return new FileSignature(FileType.MicrosoftOfficeDocument, "MS Office Document", "50-4B-03-04-14-00-06-00", 8);
            yield return new FileSignature(FileType.MicrosoftOfficeDocument, "MS Office Document", "D0-CF-11-E0-A1-B1-1A-E1", 8);
            yield return new FileSignature(FileType.Tar, "Tar", "75-73-74-61-72", 5);
            yield return new FileSignature(FileType.PDF, "PDF", "25-50-44-46-2D", 5);
            yield return new FileSignature(FileType.Zip, "Zip Bytes", "50-4B-03-04", 4);
            yield return new FileSignature(FileType.Zip, "Zip Bytes", "4C-5A-49-50", 4);
            yield return new FileSignature(FileType.Zip, "Zip Bytes", "50-4B-05-06", 4);
            yield return new FileSignature(FileType.Zip, "Zip Bytes", "50-4B-07-08", 4);
            yield return new FileSignature(FileType.Bzip2, "Bzip2 Bytes", "42-5A-68", 3);
            yield return new FileSignature(FileType.Tar, "Tar LZH Bytes", "1F-A0", 2);
            yield return new FileSignature(FileType.GZip, "GZIP File", "1F-8B", 2);
            yield return new FileSignature(FileType.Tar, "Tar LZW", "1F-9D", 2);
            yield return new FileSignature(FileType.Executable, "Executable", "4D-5A", 2);
            yield return new FileSignature(FileType.Executable, "Executable", "5A-4D", 2);
        }

        readonly FileInfo fileInfo;
        readonly int signatureMaxLength;

        public FileSignatureReader(string filePath)
        {
            fileInfo = new FileInfo(filePath);
            signatureMaxLength = Signatures()
                .Select(signature => signature.Length)
                .DefaultIfEmpty(0)
                .Max();
        }

        internal bool FileExists()
           => fileInfo.Exists;

        internal bool IsFileBlocked()
        {
            try
            {
                using (var stream = new FileStream(
                    fileInfo.FullName
                    , FileMode.Open
                    , FileAccess.Read
                    , FileShare.Read
                    , signatureMaxLength))
                    return false;
            }
            catch (SystemException e)
            {
                if (BlockedBecauseOfVirusHResults.Contains(e.HResult))
                    return true;

                throw e;
            }
        }

        internal IEnumerable<int> BlockedBecauseOfVirusHResults
            => new[] { -2147467259, -2147024671, -2147024891 };

        internal FileSignature GetFileSignature()
        {
            if (signatureMaxLength == 0)
                return FileSignature.Unknown;

            using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, signatureMaxLength))
            {
                var buffer = new byte[signatureMaxLength];
                stream.Read(buffer, 0, signatureMaxLength);

                return Signatures()
                    .Where(signature => IsSignatureMatch(buffer, signature))
                    .DefaultIfEmpty(FileSignature.Unknown)
                    .FirstOrDefault();
            }
        }

        static bool IsSignatureMatch(byte[] bytes, FileSignature signature)
        {
            if (signature.Length > bytes.Length)
                return false;

            var sameLengthBytes = new byte[signature.Length];
            Array.Copy(bytes, sameLengthBytes, signature.Length);

            return BitConverter.ToString(sameLengthBytes) == signature.Bytes;
        }

        public void Dispose()
        {
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal static class FileSignatureReader
    {
        internal const int SignatureMaxLength = 5;

        internal static IEnumerable<FileSignature> Signatures()
        {
            yield return new FileSignature(FileType.Tar, "Tar", "75-73-74-61-72", 5);
            yield return new FileSignature(FileType.Zip, "Zip Bytes", "50-4B-03-04", 4);
            yield return new FileSignature(FileType.Zip, "Zip Bytes", "4C-5A-49-50", 4);
            yield return new FileSignature(FileType.Zip, "Zip Bytes", "50-4B-05-06", 4);
            yield return new FileSignature(FileType.Zip, "Zip Bytes", "50-4B-07-08", 4);
            yield return new FileSignature(FileType.Bzip2, "Bzip2 Bytes", "42-5A-68", 3);
            yield return new FileSignature(FileType.Tar, "Tar LZH Bytes", "1F-A0", 2);
            yield return new FileSignature(FileType.GZip, "GZIP Bytes", "1F-8B", 2);
            yield return new FileSignature(FileType.Tar, "Tar LZW", "1F-9D", 2);
        }

        internal static bool IsFileBlocked(string filepath)
        {
            try
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, SignatureMaxLength))
                    return false;
            }
            catch(IOException e)
            {
                if (e.HResult == -2147024671)
                    return true;

                throw e;
            }
        }

        public static FileSignature GetFileSignature(string filepath)
        {
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, SignatureMaxLength))
            {
                var buffer = new byte[SignatureMaxLength];
                stream.Read(buffer, 0, SignatureMaxLength);


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
    }
}

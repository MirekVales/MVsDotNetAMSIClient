using System;
using System.Text;

namespace MVsDotNetAMSIClient.DetailProviders
{
    public static class EICARTestData
    {
        public const string EICARText
            = @"X5O!P%@AP[4\P" + 
            "ZX54(P^)7CC)7}$E" +
            "ICAR-STANDARD-AN" +
            "TIVIRUS-TEST-FIL" +
            "E!$H+H*";

        public const string EICARZippedBase64
            = "UEsDBAoAAAAAAOCYu" +
            "Cg8z1FoRAAAAEQAAAAJ" +
            "AAAAZWljYXIuY29tWDV" +
            "PIVAlQEFQWzRcUFpYNT" +
            "QoUF4pN0NDKTd9JEVJQ" +
            "0FSLVNUQU5EQVJELUFO" +
            "VElWSVJVUy1URVNULUZ" +
            "JTEUhJEgrSCpQSwECFA" +
            "AKAAAAAADgmLgoPM9Ra" +
            "EQAAABEAAAACQAAAAAA" +
            "AAABACAA/4EAAAAAZWl" +
            "jYXIuY29tUEsFBgAAAA" +
            "ABAAEANwAAAGsAAAAAAA==";

        public static byte[] EICARZippedBytes
            => Convert.FromBase64String(EICARZippedBase64);

        public static byte[] EICARBytes
            => Encoding.UTF8.GetBytes(EICARText);
    }
}

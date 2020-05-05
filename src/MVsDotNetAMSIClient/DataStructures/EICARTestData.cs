using System;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class EICARTestData
    {
        internal const string EICARText
            = @"X5O!P%@AP[4\P" + 
            "ZX54(P^)7CC)7}$E" +
            "ICAR-STANDARD-AN" +
            "TIVIRUS-TEST-FIL" +
            "E!$H+H*";

        internal const string EICARZipped
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

        internal static byte[] EICARZippedBytes
            => Convert.FromBase64String(EICARZipped);
    }
}

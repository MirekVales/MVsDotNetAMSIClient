using System;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Security.Cryptography;
using MVsDotNetAMSIClient.NativeMethods;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal static class Extensions
    {
        internal static void CheckResult(this int amsiMethodResult, string methodName)
        {
            if (amsiMethodResult == -2147019873)
                throw AMSIException.AMSIInvalidState;
            if (amsiMethodResult != 0)
                throw AMSIException.AMSIFailedToExecute(methodName, new Win32Exception(amsiMethodResult));
        }

        internal static void CheckHandle(this AMSIHandleContext handleContext)
        {
            if (handleContext.IsInvalid)
                throw AMSIException.FailedToInitialize();
        }

        internal static void CheckHandle(this AMSIHandleSession handleSession)
        {
            if (handleSession.IsInvalid)
                throw AMSIException.FailedToInitializeSession();
        }

        internal static void CheckResult(this int amsiMethodResult, Action<int> success, Action<int> failure)
        {
            if (amsiMethodResult != 0)
                failure(amsiMethodResult);
            else
                success(amsiMethodResult);
        }

        internal static string GetMD5Hash(this string data)
            => Encoding.UTF8.GetBytes(data).GetMD5Hash();

        internal static string GetMD5Hash(this byte[] data)
        {
            using (var hashProvider = MD5.Create())
                return string.Concat(hashProvider
                    .ComputeHash(data)
                    .Select(@byte => @byte.ToString("x2")));
        }

        internal static string GetFileMD5Hash(this FileInfo info)
        {
            using (var hashProvider = MD5.Create())
            using (var stream = File.OpenRead(info.FullName))
                return string.Concat(hashProvider
                    .ComputeHash(stream)
                    .Select(@byte => @byte.ToString("x2")));
        }

        internal static string BytesToString(this long? size)
            => (new (string name, int index)[] { ("B", 0), ("KB", 1), ("MB", 2), ("GB", 3), ("TB", 4) })
                .Where(value => size >= Math.Pow(1000, value.index))
                .Select(value => $"{Math.Round(size.Value / Math.Pow(1000, value.index), 2)} {value.name}")
                .DefaultIfEmpty("")
                .LastOrDefault();
    }
}
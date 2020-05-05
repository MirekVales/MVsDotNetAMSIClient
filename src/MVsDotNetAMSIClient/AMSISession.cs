using System;
using System.ComponentModel;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.NativeMethods;
using MVsDotNetAMSIClient.DataStructures;

namespace MVsDotNetAMSIClient
{
    internal class AMSISession : IDisposable
    {
        readonly AMSIClient client;
        readonly AMSIHandleSession sessionHandle;

        internal AMSISession(AMSIClient client)
        {
            this.client = client;
            var result = AMSIMethods.AmsiOpenSession(client.ContextHandle, out sessionHandle);
            result.CheckResult(nameof(AMSIMethods.AmsiOpenSession));
            sessionHandle.Context = client.ContextHandle;
            sessionHandle.CheckHandle();
        }

        public void Dispose()
            => sessionHandle?.Dispose();

        internal ScanResult ScanString(string content, string contentName)
        {
            using (var resultBuilder = new ResultBuilder(
                new ScanContext(
                client
                , sessionHandle
                , contentName
                , ContentType.String
                , content.Length * 4
                , content.GetMD5Hash())))
            {
                var result = AMSIMethods.AmsiScanString(client.ContextHandle, content, contentName, sessionHandle, out var resultNumber);
                ScanResult scanResult = null;
                result.CheckResult(
                    success: _ => scanResult = resultBuilder.ToResult(resultNumber)
                    , failure: _ => scanResult = resultBuilder.ToResult(new Win32Exception(result)));
                return scanResult;
            }
        }

        internal ScanResult ScanBuffer(byte[] buffer, uint length, string contentName)
        {
            using (var resultBuilder = new ResultBuilder(
                new ScanContext(
                client
                , sessionHandle
                , contentName
                , ContentType.ByteArray
                , buffer.LongLength
                , buffer.GetMD5Hash())))
            {
                var result = AMSIMethods.AmsiScanBuffer(client.ContextHandle, buffer, length, contentName, sessionHandle, out var resultNumber);
                ScanResult scanResult = null;
                result.CheckResult(
                    success: _ => scanResult = resultBuilder.ToResult(resultNumber)
                    , failure: _ => scanResult = resultBuilder.ToResult(new Win32Exception(result)));
                return scanResult;
            }
        }
    }
}

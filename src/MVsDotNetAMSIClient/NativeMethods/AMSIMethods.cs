using System;
using System.Runtime.InteropServices;

namespace MVsDotNetAMSIClient.NativeMethods
{
    internal static class AMSIMethods
    {
        internal const string AMSIDllName = "Amsi.dll";

        [DllImport(AMSIDllName, EntryPoint = nameof(AmsiInitialize), CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiInitialize([MarshalAs(UnmanagedType.LPWStr)]string appName, out AMSIHandleContext amsiContext);

        [DllImport(AMSIDllName, EntryPoint = nameof(AmsiUninitialize), CallingConvention = CallingConvention.StdCall)]
        internal static extern void AmsiUninitialize(IntPtr amsiContext);

        [DllImport(AMSIDllName, EntryPoint = nameof(AmsiOpenSession), CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiOpenSession(
            AMSIHandleContext amsiContext,
            out AMSIHandleSession session);

        [DllImport(AMSIDllName, EntryPoint = nameof(AmsiCloseSession), CallingConvention = CallingConvention.StdCall)]
        internal static extern void AmsiCloseSession(
            AMSIHandleContext amsiContext,
            IntPtr session);

        [DllImport(AMSIDllName, EntryPoint = nameof(AmsiScanString), CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiScanString(AMSIHandleContext amsiContext, 
            [In()] [MarshalAs(UnmanagedType.LPWStr)]string @string, 
            [In()] [MarshalAs(UnmanagedType.LPWStr)]string contentName,
            AMSIHandleSession session,
            out int result);

        [DllImport(AMSIDllName, EntryPoint = nameof(AmsiScanBuffer), CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiScanBuffer(
            AMSIHandleContext amsiContext
            , byte[] buffer
            , ulong length
            , string contentName
            , AMSIHandleSession session
            , out int result);

        internal static bool IsDllImportPossible()
        {
            try
            {
                Marshal.PrelinkAll(typeof(AMSIMethods));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

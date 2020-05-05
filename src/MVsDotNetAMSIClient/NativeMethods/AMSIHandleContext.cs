using Microsoft.Win32.SafeHandles;

namespace MVsDotNetAMSIClient.NativeMethods
{
    internal sealed class AMSIHandleContext : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal AMSIHandleContext() : base(true)
        { }

        protected override bool ReleaseHandle()
        {
            AMSIMethods.AmsiUninitialize(handle);
            return true;
        }
    }
}

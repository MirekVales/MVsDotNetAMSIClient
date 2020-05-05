using Microsoft.Win32.SafeHandles;

namespace MVsDotNetAMSIClient.NativeMethods
{
    internal sealed class AMSIHandleSession : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal AMSIHandleContext Context { get; set; }
        public override bool IsInvalid => base.IsInvalid || Context.IsInvalid;

        internal AMSIHandleSession() : base(true)
        { }

        protected override bool ReleaseHandle()
        {
            AMSIMethods.AmsiCloseSession(Context, handle);
            return true;
        }
    }
}

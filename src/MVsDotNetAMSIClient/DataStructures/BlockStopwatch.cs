using System;
using System.Diagnostics;

namespace MVsDotNetAMSIClient.DataStructures
{
    internal class BlockStopwatch : IDisposable
    {
        readonly Action<TimeSpan> onClosed;
        readonly Stopwatch stopwatch;

        internal DateTime Start { get; }

        internal TimeSpan Elapsed
            => stopwatch.Elapsed;

        internal BlockStopwatch()
        {
            Start = DateTime.UtcNow;
            stopwatch = Stopwatch.StartNew();
        }

        internal BlockStopwatch(Action<TimeSpan> onClosed)
        {
            this.onClosed = onClosed;
            Start = DateTime.UtcNow;
            stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
            => onClosed?.Invoke(stopwatch.Elapsed);
    }
}

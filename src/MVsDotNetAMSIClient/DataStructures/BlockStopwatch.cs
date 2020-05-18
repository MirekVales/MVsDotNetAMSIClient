using System;
using System.Diagnostics;

namespace MVsDotNetAMSIClient.DataStructures
{
    public class BlockStopwatch : IDisposable
    {
        readonly Action<TimeSpan> onClosed;
        readonly Stopwatch stopwatch;

        public DateTime Start { get; }

        public TimeSpan Elapsed
            => stopwatch.Elapsed;

        public BlockStopwatch()
        {
            Start = DateTime.UtcNow;
            stopwatch = Stopwatch.StartNew();
        }

        public BlockStopwatch(Action<TimeSpan> onClosed) : this()
        {
            this.onClosed = onClosed;
        }

        public void Dispose()
            => onClosed?.Invoke(stopwatch.Elapsed);
    }
}

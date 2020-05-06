using System;

namespace MVsDotNetAMSIClient.DataStructures.Streams
{
    internal interface IInputStream : IDisposable
    {
        string CurrentEntryName { get; }
        long Position { get; }

        int Read(byte[] buffer, int numberOfBytes);
        void ScanTo(byte[] buffer, long offset);
    }
}
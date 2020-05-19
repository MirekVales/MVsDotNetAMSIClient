using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts;

namespace MVsDotNetAMSIClient.UnitTests
{
    internal class TestItemsBag : IEnumerable<ScanResult>, IDisposable
    {
        readonly AMSIClient client;
        readonly List<Func<AMSISession, ScanResult>> sessionItems
            = new List<Func<AMSISession, ScanResult>>();
        readonly List<Func<AMSIClient, ScanResult>> clientItems
            = new List<Func<AMSIClient, ScanResult>>();
        readonly HashSet<string> temporaryFile
            = new HashSet<string>();

        public TestItemsBag(AMSIClient client)
            => this.client = client;

        internal TestItemsBag SessionTest(Func<AMSISession, ScanResult> item)
        {
            sessionItems.Add(item);
            return this;
        }

        internal TestItemsBag ClientTest(Func<AMSIClient, ScanResult> item)
        {
            clientItems.Add(item);
            return this;
        }

        internal string CreateTemporaryFile(byte[] content)
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            File.WriteAllBytes(path, content);
            temporaryFile.Add(path);

            return path;
        }

        public void Dispose()
        {
            foreach (var file in temporaryFile)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

        }

        public IEnumerator<ScanResult> GetEnumerator()
        {
            foreach (var item in clientItems)
                yield return item(client);

            foreach (var item in sessionItems)
                using (var session = client.CreateSession())
                    yield return item(session);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
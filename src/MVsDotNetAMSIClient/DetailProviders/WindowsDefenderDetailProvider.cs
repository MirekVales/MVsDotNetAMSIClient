using Polly;
using System;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using MVsDotNetAMSIClient.Contracts;
using System.Diagnostics.Eventing.Reader;
using MVsDotNetAMSIClient.DataStructures;

namespace MVsDotNetAMSIClient.DetailProviders
{
    internal class WindowsDefenderDetailProvider : IDetailProvider
    {
        public IScanResultDetail GetDetail(ResultBuilder builder)
        {
            var appName = "";
            using (var process = Process.GetCurrentProcess())
                appName = process.MainModule.FileName;
            var minimalTime = builder.Start.AddSeconds(-1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var queryString = $"*[System/TimeCreated[@SystemTime>'{minimalTime}'] " +
                $"and System/EventID=1116 " +
                $"and EventData/Data[@Name='Process Name']='{appName}']";

            const int maxNumberOfRetrySteps = 5;
            var stepMaxRetrievalTime = Math.Max(
                10
                , builder.ScanContext.Client.Configuration.MaximumScanResultRetrievalTime.TotalMilliseconds / maxNumberOfRetrySteps);

            return Policy
                .HandleResult<WindowsDefenderDetail>(detail => detail == null)
                .WaitAndRetry(maxNumberOfRetrySteps, iteration => TimeSpan.FromMilliseconds(stepMaxRetrievalTime))
                .Execute(() => GetDetail(queryString));
        }

        WindowsDefenderDetail GetDetail(string queryString)
        {
            var query = new EventLogQuery("Microsoft-Windows-Windows Defender/Operational", PathType.LogName, queryString);
            using (var reader = new EventLogReader(query))
            {
                EventRecord eventInstance = reader.ReadEvent();
                try
                {
                    while (eventInstance != null)
                    {
                        var instance = reader.ReadEvent();
                        if (instance == null)
                            break;
                        eventInstance = instance;
                    }

                    return ParseData(eventInstance);
                }
                finally
                {
                    if (eventInstance != null)
                        eventInstance.Dispose();
                }
            }
        }

        WindowsDefenderDetail ParseData(EventRecord eventInstance)
        {
            if (eventInstance == null)
                return null;

            var xml = XDocument.Parse(eventInstance.ToXml());
            var ns = xml.Root.Name.Namespace;
            var eventData = xml.Root.Element(ns + "EventData");
            Func<string, string> getString = GetString(ns, eventData);
            Func<string, int> getInteger = GetInteger(getString);

            return new WindowsDefenderDetail()
            {
                EventID = eventInstance.Id,
                CategoryID = getInteger(nameof(WindowsDefenderDetail.CategoryID)),
                CategoryName = getString(nameof(WindowsDefenderDetail.CategoryName)),
                EngineVersion = getString(nameof(WindowsDefenderDetail.EngineVersion)),
                FWLink = getString(nameof(WindowsDefenderDetail.FWLink)),
                ProductVersion = getString(nameof(WindowsDefenderDetail.ProductVersion)),
                SecurityIntelligenceVersion = getString(nameof(WindowsDefenderDetail.SecurityIntelligenceVersion)),
                SeverityID = getInteger(nameof(WindowsDefenderDetail.SeverityID)),
                SeverityName = getString(nameof(WindowsDefenderDetail.SeverityName)),
                ThreatID = getString(nameof(WindowsDefenderDetail.ThreatID)),
                ThreatName = getString(nameof(WindowsDefenderDetail.ThreatName)),
                StatusCode = getInteger(nameof(WindowsDefenderDetail.StatusCode)),
                StatusDescription = getString(nameof(WindowsDefenderDetail.StatusDescription))
            };
        }

        static Func<string, int> GetInteger(Func<string, string> getString)
            => name =>
            int.TryParse(getString(name), out var result)
            ? result
            : 0;

        static Func<string, string> GetString(XNamespace ns, XElement eventData)
            => name => eventData?
                            .Elements(ns + "Data")
                            .FirstOrDefault(
                                data =>
                                    data.Attributes().Any(
                                        a => string.Compare(a.Value.Replace(" ", "")
                                        , name
                                        , StringComparison.InvariantCultureIgnoreCase) == 0))
                            ?.Value;
    }
}

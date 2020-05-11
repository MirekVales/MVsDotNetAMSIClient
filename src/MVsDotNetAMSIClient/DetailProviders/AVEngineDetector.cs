using System;
using System.Linq;
using System.Management;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient.DetailProviders
{
    internal static class AVEngineDetector
    {
        internal readonly static Dictionary<Guid, (DetectionEngine DetectionEngine, Func<uint, bool> IsEnabled)> EngineInfos
            = new Dictionary<Guid, (DetectionEngine DetectionEngine, Func<uint, bool> IsEnabled)>()
            {
                { new Guid("17AD7D40-BA12-9C46-7131-94903A54AD8B"), (DetectionEngine.Avast, state => true) },
                { new Guid("0E9420C4-06B3-7FA0-3AB1-6E49CB52ECD9"), (DetectionEngine.AVG, state => true) },
                { new Guid("92356E98-E159-03AA-2BF0-6FE55F131038"), (DetectionEngine.BitDefender, state => state != 270336) },
                { new Guid("ea21bce8-a461-99c3-3a0d-4c964e75494e"), (DetectionEngine.BitDefenderFree, state => state != 270336) },
                { new Guid("77DEAFED-8149-104B-25A1-21771CA47CD1"), (DetectionEngine.ESET, state => true) },
                { new Guid("ec1d6f37-e411-475a-df50-12ff7fe4ac70"), (DetectionEngine.ESETSecurity, state => true) },
                { new Guid("E5E70D32-0101-4F12-8FB0-D96ACA4F34C0"), (DetectionEngine.ESETSecurity, state => true) },
                { new Guid("545C8713-0744-B079-87F8-349A6D5C8CF0"), (DetectionEngine.GDataAntivirus, state => true) },
                { new Guid("AE1D740B-8F0F-D137-211D-873D44B3F4AE"), (DetectionEngine.KasperskyAntivirus, state => true) },
                { new Guid("86367591-4BE4-AE08-2FD9-7FCB8259CD98"), (DetectionEngine.KasperskyEndpointSecurity, state => true) },
                { new Guid("63DF5164-9100-186D-2187-8DC619EFD8BF"), (DetectionEngine.Norton360, state => true) },
                { new Guid("A2708B76-6835-6565-CB96-694212954A75"), (DetectionEngine.NortonSecurity, state => true) },
                { new Guid("D68DDC3A-831F-4fae-9E44-DA132C1ACF46"), (DetectionEngine.WindowsDefender, state => state != 393472) },
            };

        internal static DetectionEngineInfo DetectEngine()
            => DetectEngine(EngineInfos);

        internal static IEnumerable<DetectionEngineInfo> ListDetectionEngines()
            => ListDetectionEngines(EngineInfos);

        internal static IEnumerable<DetectionEngineInfo> ListDetectionEngines(
            IDictionary<Guid, (DetectionEngine DetectionEngine, Func<uint, bool> IsEnabled)> engineInfos)
        {
            var engines = new HashSet<DetectionEngineInfo>();
            try
            {
                string wmiScope = $@"\\{Environment.MachineName}\root\SecurityCenter2";
                using (var searcher = new ManagementObjectSearcher(wmiScope, "SELECT * FROM AntiVirusProduct"))
                    foreach (ManagementObject engine in searcher.Get())
                        using (engine)
                        {
                            ParseRecord(engineInfos, engines, engine);
                        }
            }
            catch
            {}

            return engines;
        }

        private static void ParseRecord(
            IDictionary<Guid, (DetectionEngine DetectionEngine, Func<uint, bool> IsEnabled)> engineInfos
            , HashSet<DetectionEngineInfo> foundRecords
            , ManagementObject record)
        {
            var instanceGuid = new Guid((string)record["instanceGuid"]);
            var productState = (uint)record["productState"];
            var displayName = (string)record["displayName"];
            string companyName = null;
            string pathToSignedProductExe = null;
            string versionInfo = null;

            if (engineInfos.TryGetValue(instanceGuid, out var engineInfo))
            {
                foreach (var property in record.Properties)
                {
                    if (property.Name == nameof(companyName))
                        companyName = (string)property.Value;
                    else if (property.Name == nameof(pathToSignedProductExe))
                        pathToSignedProductExe = (string)property.Value;
                    else if (property.Name == nameof(versionInfo))
                        versionInfo = (string)property.Value;
                }

                foundRecords.Add(new DetectionEngineInfo()
                {
                    EngineType = engineInfo.DetectionEngine,
                    InstanceGuid = instanceGuid,
                    DisplayName = displayName,
                    CompanyName = companyName,
                    PathToSignedProductExe = pathToSignedProductExe,
                    VersionInfo = versionInfo,
                    ProductState = productState,
                    IsEnabled = engineInfo.IsEnabled(productState)
                });
            }
        }

        internal static DetectionEngineInfo DetectEngine(
            IDictionary<Guid, (DetectionEngine DetectionEngine, Func<uint, bool> IsEnabled)> engineInfos)
            => ListDetectionEngines(engineInfos)
                    .Where(engine => engine.IsEnabled)
                    .DefaultIfEmpty(DetectionEngineInfo.Empty)
                    .FirstOrDefault();
    }
}

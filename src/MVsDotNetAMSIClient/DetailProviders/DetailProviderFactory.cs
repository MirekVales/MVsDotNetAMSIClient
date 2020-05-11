using System;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient.DetailProviders
{
    internal static class DetailProviderFactory
    {
        internal static IDetailProvider GetDetailProvider(DetectionEngine detectionEngine)
        {
            var providers = new Dictionary<DetectionEngine, Func<IDetailProvider>>()
            {
                { DetectionEngine.WindowsDefender, () => new WindowsDefenderDetailProvider()  }
            };

            return providers.TryGetValue(detectionEngine, out var factory)
                ? factory()
                : null;
        }
    }
}

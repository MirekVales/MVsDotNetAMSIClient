using System;
using System.Collections.Generic;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.DataStructures;
using MVsDotNetAMSIClient.DetailProviders;
using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient
{
    public class Scan
    {
        public AMSIClientConfiguration Configuration { get; set; }
        public int? ScanRetryAttempts { get; set; }
        public TimeSpan ScanRetryAttemptDelay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration">AMSI client configuration</param>
        /// <param name="scanRetryAttempts">
        /// Scan that ends up with application exception will be repeated
        /// until correct result is received or max number of attempts is reached</param>
        /// <param name="scanRetryAttemptDelay"> If ScanRetryMaxAttempts is enabled,
        /// this delay is applied between attempts</param>
        public Scan(AMSIClientConfiguration configuration, int? scanRetryAttempts, TimeSpan scanRetryAttemptDelay)
        {
            Configuration = configuration;
            ScanRetryAttempts = scanRetryAttempts;
            ScanRetryAttemptDelay = scanRetryAttemptDelay;
        }

        public Scan() : this(AMSIClientConfiguration.Default, 1, TimeSpan.FromSeconds(1))
        { }

        public bool IsAvailable()
            => AMSIClient.IsAvailable();

        ScanResult Execute(Func<ScanResult> action)
            => action.ExecuteInRetryPolicy(
                    result => result.Result == DetectionResult.ApplicationError
                    , ScanRetryAttempts ?? 1
                    , ScanRetryAttemptDelay);

        public ScanResult String(string content, string contentName)
            => Execute(() =>
            {
                using (var client = AMSIClient.Create(Configuration))
                using (var session = client.CreateSession())
                    return session.ScanString(content, contentName);
            });

        public ScanResult Buffer(byte[] buffer, int length, string contentName)
            => Buffer(buffer, (uint)length, contentName);

        public ScanResult Buffer(byte[] buffer, uint length, string contentName)
            => Execute(() =>
            {
                using (var client = AMSIClient.Create(Configuration))
                using (var session = client.CreateSession())
                    return session.ScanBuffer(buffer, length, contentName);
            });

        public ScanResult File(string filePath)
            => Execute(() =>
            {
                using (var client = AMSIClient.Create(Configuration))
                using (var session = client.CreateSession())
                    return session.ScanFile(filePath);
            });

        public ScanResult EICARString()
            => Execute(() =>
            {
                using (var client = AMSIClient.Create(Configuration))
                    return client.ScanString(EICARTestData.EICARText, nameof(EICARTestData.EICARText));
            });

        public ScanResult EICARByteArray()
            => Execute(() =>
            {
                using (var client = AMSIClient.Create(Configuration))
                    return client.ScanBuffer(
                    EICARTestData.EICARZippedBytes
                    , (uint)EICARTestData.EICARZippedBytes.Length
                    , nameof(EICARTestData.EICARZippedBytes));
            });

        public IEnumerable<DetectionEngineInfo> ListDetectionEngines()
            => AVEngineDetector.ListDetectionEngines();
    }
}

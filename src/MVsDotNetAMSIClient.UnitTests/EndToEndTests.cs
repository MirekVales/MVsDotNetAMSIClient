using System.Text;
using NUnit.Framework;
using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.Contracts.Enums;
using MVsDotNetAMSIClient.DetailProviders;

namespace MVsDotNetAMSIClient.UnitTests
{
    public class EndToEndTests
    {
        [Test]
        public void DetectsEICAR()
        {
            // ARRANGE

            using (var client = AMSIClient.Create())
            using (var testItems = new TestItemsBag(client))
            {
                testItems
                    .ClientTest(c => c.TestEICARString())
                    .ClientTest(c => c.TestEICARByteArray())
                    .ClientTest(c => c.ScanString(EICARTestData.EICARText, nameof(EICARTestData.EICARText)))
                    .ClientTest(c => c.ScanBuffer(
                        EICARTestData.EICARZippedBytes
                        , EICARTestData.EICARZippedBytes.Length
                        , nameof(EICARTestData.EICARZippedBytes)))
                    .ClientTest(c => c.ScanFile(testItems.CreateTemporaryFile(EICARTestData.EICARZippedBytes)))
                    .SessionTest(c => c.ScanString(EICARTestData.EICARText, nameof(EICARTestData.EICARText)))
                    .SessionTest(c => c.ScanBuffer(
                        EICARTestData.EICARZippedBytes
                        , EICARTestData.EICARZippedBytes.Length
                        , nameof(EICARTestData.EICARZippedBytes)))
                    .SessionTest(c => c.ScanFile(testItems.CreateTemporaryFile(EICARTestData.EICARZippedBytes)));

                // ACT

                foreach (var result in testItems)
                {

                    // ASSERT

                    Assert.IsFalse(result.IsSafe);
                    Assert.IsTrue(
                        result.Result == DetectionResult.IdentifiedAsMalware
                        || result.Result == DetectionResult.FileBlocked);
                    Assert.IsTrue(HasValidData(result));
                }
            }
        }

        [Test]
        public void ValidatesSafeContent()
        {
            const string SafeString = "A safe array of characters without any risk";
            byte[] safeBytes = Encoding.UTF8.GetBytes(SafeString);

            // ARRANGE

            using (var client = AMSIClient.Create())
            using (var testItems = new TestItemsBag(client))
            {
                testItems
                    .ClientTest(c => c.ScanString(SafeString, nameof(SafeString)))
                    .ClientTest(c => c.ScanBuffer(safeBytes, safeBytes.Length, nameof(safeBytes)))
                    .ClientTest(c => c.ScanFile(testItems.CreateTemporaryFile(safeBytes)))
                    .SessionTest(c => c.ScanString(SafeString, nameof(SafeString)))
                    .SessionTest(c => c.ScanBuffer(safeBytes, safeBytes.Length, nameof(safeBytes)))
                    .SessionTest(c => c.ScanFile(testItems.CreateTemporaryFile(safeBytes)));

                // ACT

                foreach (var result in testItems)
                {

                    // ASSERT

                    Assert.IsTrue(result.IsSafe);
                    Assert.IsTrue(result.Result == DetectionResult.NotDetected || result.Result == DetectionResult.Clean);
                    Assert.IsTrue(HasValidData(result));
                }
            }
        }

        bool HasValidData(ScanResult result)
        {
            Assert.NotNull(result.TimeStamp);
            Assert.NotNull(result.Result);
            Assert.NotNull(result.ContentInfo);
            Assert.NotNull(result.DetectionEngineInfo);
            Assert.NotNull(result.ContentInfo.ContentType);
            Assert.NotNull(result.ContentInfo.ContentName);
            Assert.NotNull(result.DetectionEngineInfo.ClientProcessAppName);
            Assert.NotNull(result.DetectionEngineInfo.ClientProcessUsername);
            Assert.NotNull(result.DetectionEngineInfo.DetectionEngine);
            Assert.NotNull(result.DetectionEngineInfo.EnvironmentMachineName);
            Assert.NotNull(result.DetectionEngineInfo.EnvironmentOSDescription);

            return true;
        }
    }
}
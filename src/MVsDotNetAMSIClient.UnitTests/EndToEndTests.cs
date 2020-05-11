using NUnit.Framework;
using MVsDotNetAMSIClient.Contracts.Enums;

namespace MVsDotNetAMSIClient.UnitTests
{
    public class EndToEndTests
    {
        [Test]
        public void DetectsEICAR()
        {
            using (var client = AMSIClient.Create())
            {
                var result = client.TestEICARString();
                Assert.AreEqual(DetectionResult.IdentifiedAsMalware, result.Result);
            }
        }

        [Test]
        public void LeavesSafeString()
        {
            const string safeString = "A safe string of character without any risk";

            using (var client = AMSIClient.Create())
            {
                var result = client.ScanString(safeString, nameof(safeString));
                Assert.AreEqual(DetectionResult.NotDetected, result.Result);
            }
        }
    }
}
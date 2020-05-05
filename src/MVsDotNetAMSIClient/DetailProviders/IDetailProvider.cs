using MVsDotNetAMSIClient.Contracts;
using MVsDotNetAMSIClient.DataStructures;

namespace MVsDotNetAMSIClient.DetailProviders
{
    internal interface IDetailProvider
    {
        IScanResultDetail GetDetail(ResultBuilder builder);
    }
}
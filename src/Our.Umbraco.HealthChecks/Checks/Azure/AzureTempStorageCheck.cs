using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Azure
{
    [HealthCheck("F9088377-103A-4712-B428-D4AB6E5B2A67", "Azure Temp Storage Config Check",
    Description = "Checks that temp storage config is appropriate for the Azure platform.",
    Group = "Azure")]
    public class AzureTempStorageCheck : HealthCheck
    {

        public AzureTempStorageCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckTempStorage() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("UmbracoPathCheck has no executable actions");
        }

        private static HealthCheckStatus CheckTempStorage()
        {
            var umbracoVersion = WebConfigurationManager.AppSettings["umbracoConfigurationStatus"].Split('.');

            var umbMajorVersion = int.Parse(umbracoVersion[0]);
            var umbMinorVersion = int.Parse(umbracoVersion[1]);

            var matchingValue = umbMinorVersion >= 7 ? "EnvironmentTemp" : "true";
            var appSetting = umbMinorVersion >= 7
                ? "umbracoLocalTempStorage"
                : umbMinorVersion >= 6
                    ? "umbracoContentXMLStorage"
                    : "umbracoContentXMLUseLocalTemp";

            var tempStorageSetting = WebConfigurationManager.AppSettings[appSetting];

            var isCorrectValue = string.Equals(tempStorageSetting, matchingValue, StringComparison.InvariantCultureIgnoreCase);

            return
                new HealthCheckStatus(isCorrectValue ? "Success" : $"{appSetting} should be set to '{matchingValue}', but is " + (!string.IsNullOrWhiteSpace(tempStorageSetting) ? $"currently set to '{tempStorageSetting}'" : "missing from the app settings"))
                {
                    ResultType = isCorrectValue ? StatusResultType.Success : StatusResultType.Error,
                    Actions = new List<HealthCheckAction>()
                };
        }

    }
}

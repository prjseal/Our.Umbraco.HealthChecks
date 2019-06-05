using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Azure
{
    [HealthCheck("F9088377-103A-4712-B428-D4AB6E5B2A67", "Azure Temp Storage Config Check - (from Our.Umbraco.HealthChecks)",
    Description = "Checks that temp storage config is appropriate for the Azure platform.",
    Group = "Azure")]
    public class AzureTempStorageCheck : HealthCheck
    {
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
            var matchingValue = "EnvironmentTemp";
            var appSetting = "umbracoLocalTempStorage";

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

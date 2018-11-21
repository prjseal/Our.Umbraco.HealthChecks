using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Our.Umbraco.HealthChecks.Checks.Azure
{
    [HealthCheck("EA9619FE-1DF4-4399-A4E5-32F2CF0CDC1F", "Azure File Change Notification Config Check",
        Description = "Checks that fcnMode config is appropriate for the Azure platform.",
        Group = "Azure")]
    public class AzureFcnModeCheck : AbstractConfigCheck
    {
        public AzureFcnModeCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {

        }

        public override string FilePath
        {
            get { return "~/Web.config"; }
        }

        public override string XPath
        {
            get { return "/configuration/system.web/httpRuntime/@fcnMode"; }
        }

        public override ValueComparisonType ValueComparisonType
        {
            get { return ValueComparisonType.ShouldEqual; }
        }

        public override IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                return new List<AcceptableConfiguration>
                {
                    new AcceptableConfiguration { IsRecommended = true, Value = "Single" }
                };
            }
        } 

        public override string CheckSuccessMessage
        {
            get { return $"fcnMode is set to '{CurrentValue}'"; }
        }

        public override string CheckErrorMessage
        {
            get { return $"fcnMode should be set to '{Values.First(v => v.IsRecommended).Value}', but is currently set to '{CurrentValue}'"; }
        }

        public override string RectifySuccessMessage
        {
            get { return $"fcnMode set to '{Values.First(v => v.IsRecommended).Value}'"; }
        }
    }
}

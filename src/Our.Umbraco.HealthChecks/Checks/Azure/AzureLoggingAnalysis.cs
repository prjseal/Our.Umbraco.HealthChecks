using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Our.Umbraco.HealthChecks.Checks.Azure
{
    [HealthCheck("D0E1A87E-5EC4-426D-8E2B-C76AE7350439", "Azure Logging Check",
        Description = "Checks that logging patterns are appropriate for the Azure platform.",
        Group = "Azure")]
    public class AzureLoggingAnalysis : AbstractConfigCheck
    {
        public AzureLoggingAnalysis(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {

        }

        public override string FilePath
        {
            get { return "~/config/log4net.config"; }
        }

        public override string XPath
        {
            get { return "/log4net/root/priority/@value"; }
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
                    new AcceptableConfiguration {IsRecommended = true, Value = "Error"},
                    new AcceptableConfiguration {IsRecommended = false, Value = "Warn"},
                };
            }
        }



        public override string CheckSuccessMessage
        {
            get
            {
                var isRecommended = CurrentValue == Values.First(v => v.IsRecommended).Value;
                return isRecommended
                    ? $"Log4Net priority is set to '{CurrentValue}'"
                    : $"Log4Net priority is set to '{CurrentValue}', consider setting it to '{Values.First(v => v.IsRecommended).Value}' for optimum performance";
            }
        }


        public override string CheckErrorMessage
        {
            get
            {
                return
                    $"Log4Net priority should be set to '{Values.First(v => v.IsRecommended).Value}', but is currently set to '{CurrentValue}'";
            }
        }

        public override string RectifySuccessMessage
        {
            get { return $"Log4Net priority set to '{Values.First(v => v.IsRecommended).Value}'"; }
        }
    }
}

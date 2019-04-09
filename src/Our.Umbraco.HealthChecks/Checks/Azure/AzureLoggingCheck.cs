using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Our.Umbraco.HealthChecks.Checks.Azure
{
    [HealthCheck("D0E1A87E-5EC4-426D-8E2B-C76AE7350439", "Azure Logging Check - (from Our.Umbraco.HealthChecks)",
        Description = "Checks that logging patterns are appropriate for the Azure platform.",
        Group = "Azure")]
    public class AzureLoggingCheck : AbstractConfigCheck
    {
        public AzureLoggingCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
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
                    new AcceptableConfiguration {IsRecommended = true, Value = "Warn"},
                };
            }
        }
        
        public override string CheckSuccessMessage
        {
            get
            {
                bool isRecommended = AcceptableValues().Select(x => x.ToLower()).Contains(CurrentValue?.ToLower());
                                                            
                return isRecommended
                    ? $"Log4Net priority is set to '{CurrentValue}'"
                    : $"Log4Net priority is set to '{CurrentValue}', consider setting it to one of '{AccpetableValuesMessage()}' for optimum performance";
            }
        }


        public override string CheckErrorMessage
        {
            get
            {
                return
                    $"Log4Net priority should be set to one of '{AccpetableValuesMessage()}', but is currently set to '{CurrentValue}'";
            }
        }

        public override string RectifySuccessMessage
        {
            get { return $"Log4Net priority set to '{Values.First(v => v.IsRecommended).Value}'"; }
        }

        private IEnumerable<string> AcceptableValues()
        {
            return Values.Where(x => x.IsRecommended).Select(x => x.Value);
        }

        private string AccpetableValuesMessage()
        {
            return string.Join(", ", AcceptableValues());
        }
    }
}

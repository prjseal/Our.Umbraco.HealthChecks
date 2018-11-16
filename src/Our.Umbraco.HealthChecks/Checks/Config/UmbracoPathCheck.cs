using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Config
{
    [HealthCheck("467EFE42-E37D-47FE-A75F-E2D7D2D98438", "Umbraco Path Check",
    Description = "Checks to see if you have changed the umbraco path.",
    Group = "Configuration")]
    public class UmbracoPathCheck : HealthCheck
    {
        public UmbracoPathCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckUmbracoPath() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("UmbracoPathCheck has no executable actions");
        }

        private HealthCheckStatus CheckUmbracoPath()
        {
            var umbracoPath = WebConfigurationManager.AppSettings["umbracoPath"];
            var umbracoReservedPaths = WebConfigurationManager.AppSettings["umbracoReservedPaths"];

            bool success = false;
            StringBuilder message = new StringBuilder();
            if(umbracoPath != "~/umbraco")
            {
                message.Append("You have changed the umbraco path from the default.");
                if (!umbracoReservedPaths.Split(',').Contains(umbracoPath))
                {
                    message.Append("Your umbraco path is not in the <strong>umbracoReservedPaths</strong> appSetting value.");
                }
                else
                {
                    success = true;
                }
            }
            else
            {
                message.Append("You should consider changing the umbraco path for security reasons.<br/>");
                message.Append("<br/>");
                message.Append("<ol>");
                message.Append("<li>In the appSettings, change the value of <strong>umbracoPath</strong> to something not obvious to hackers like <strong>my-secret-backoffice-url</strong> (use <strong>-</strong> instead of spaces.)</li>");
                message.Append("<li>Add this to the other appSetting <strong>umbracoReservedPaths</strong>. You can replace <strong>~/umbraco</strong></li>");
                message.Append("<li>Change the name of the umbraco folder to be the same name.</li>");
                message.Append("<li>Test that everything still works.</li>");
                message.Append("<ol>");
                message.Append("<br/>");
                message.Append("<strong>BEWARE</strong> This could break some packages which rely on the default umbraco path, so test it first.");
                message.Append("<br/>");
            }
            
            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message.ToString())
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }
    }
}

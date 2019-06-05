using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Config
{
    [HealthCheck("467EFE42-E37D-47FE-A75F-E2D7D2D98438", "Umbraco Path Check - (from Our.Umbraco.HealthChecks)",
    Description = "Checks to see if you have changed the umbraco path.",
    Group = "Configuration")]
    public class UmbracoPathCheck : HealthCheck
    {
        protected readonly ILocalizedTextService _textService;

        public UmbracoPathCheck(ILocalizedTextService textService)
        {
            _textService = textService;
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
                message.Append(_textService.Localize("Our.Umbraco.HealthChecks/umbracoPathCheckPathChanged"));
                if (!umbracoReservedPaths.Split(',').Contains(umbracoPath))
                {
                    message.Append(_textService.Localize("Our.Umbraco.HealthChecks/umbracoPathCheckReservedPathMissing"));
                }
                else
                {
                    success = true;
                }
            }
            else
            {
                message.Append(_textService.Localize("Our.Umbraco.HealthChecks/umbracoPathCheckConsiderChangingPath"));
                message.Append("<br/><br/>");
                message.Append("<ol>");
                message.AppendFormat("<li>{0}</li>", _textService.Localize("Our.Umbraco.HealthChecks/umbracoPathCheckInstructions1"));
                message.AppendFormat("<li>{0}</li>", _textService.Localize("Our.Umbraco.HealthChecks/umbracoPathCheckInstructions2"));
                message.AppendFormat("<li>{0}</li>", _textService.Localize("Our.Umbraco.HealthChecks/umbracoPathCheckInstructions3"));
                message.AppendFormat("<li>{0}</li>", _textService.Localize("Our.Umbraco.HealthChecks/umbracoPathCheckInstructions4"));
                message.Append("<ol>");
                message.Append("<br/>");
                message.Append(_textService.Localize("Our.Umbraco.HealthChecks/umbracoPathCheckWarning"));
                message.Append("<br/>");
            }
            
            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message.ToString())
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Warning,
                    Actions = actions
                };
        }
    }
}

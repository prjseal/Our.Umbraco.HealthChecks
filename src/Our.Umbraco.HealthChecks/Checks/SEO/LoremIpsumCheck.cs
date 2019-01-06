using Examine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Configuration;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.SEO
{
    [HealthCheck("ADB911FE-9322-4711-AD2E-42E1FB7BC577", "Lorem Ipsum Check - (from Our.Umbraco.HealthChecks)",
    Description = "Checks to see if you have any Lorem Ipsum content in your site.",
    Group = "SEO")]
    public class LoremIpsumCheck : HealthCheck
    {
        protected readonly ILocalizedTextService TextService;

        public LoremIpsumCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            TextService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckForLoremIpsumContent() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("LoremIpsumCheck has no executable actions");
        }

        private HealthCheckStatus CheckForLoremIpsumContent()
        {
            var queryFromAppSetting = WebConfigurationManager.AppSettings["HealthCheck.SEO.LoremIpsum.SearchTerms"];
            var query = string.IsNullOrEmpty(queryFromAppSetting) ? TextService.Localize("Our.Umbraco.HealthChecks/loremIpsumCheckSearchTerms") : queryFromAppSetting;

            var searcher = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"];
            var results = searcher.Search(query, false);
            var success = results.TotalItemCount == 0;

            StringBuilder message = new StringBuilder();
            if (success)
            {
                message.Append(TextService.Localize("Our.Umbraco.HealthChecks/loremIpsumCheckSuccessNoMatchingContent"));
                message.AppendFormat("<strong>{0}</strong>", query);
            }
            else
            {
                message.Append(TextService.Localize("Our.Umbraco.HealthChecks/loremIpsumCheckErrorMatchingContentFound"));
                message.AppendFormat("<strong>{0}</strong>", query);
                message.Append("<br/>");
                message.Append("<br/>");
                message.Append("<ul>");
                foreach (var result in results)
                {
                    message.Append("<li>");
                    message.AppendFormat(TextService.Localize("Our.Umbraco.HealthChecks/loremIpsumCheckResultItemText"), result.Id, result.Fields["nodeName"], result.Fields["urlName"]);
                    message.Append("</li>");
                }
                message.Append("</ul>");
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
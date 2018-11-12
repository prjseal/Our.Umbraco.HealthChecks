using Examine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Configuration;

namespace Umbraco.Web.HealthCheck.Checks.SEO
{
    [HealthCheck("ADB911FE-9322-4711-AD2E-42E1FB7BC577", "Lorem Ipsum Check",
    Description = "Checks to see if you have any Lorem Ipsum content in your site.",
    Group = "SEO")]
    public class LoremIpsum : HealthCheck
    {
        public LoremIpsum(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckForLoremIpsumContent() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "CheckForLoremIpsumContent":
                    return CheckForLoremIpsumContent();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckForLoremIpsumContent()
        {
            var queryFromAppSetting = WebConfigurationManager.AppSettings["HealthCheck.SEO.LoremIpsum.SearchTerms"];
            var query = string.IsNullOrEmpty(queryFromAppSetting) ? "lorem ipsum dolor amet" : queryFromAppSetting;

            var searcher = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"];
            var results = searcher.Search(query, false);
            var success = results.TotalItemCount == 0;

            StringBuilder message = new StringBuilder();
            if (success)
            {
                message.Append("There is no content matching these search terms: ");
                message.AppendFormat("<strong>{0}</strong>", query);message.AppendFormat("<strong>{0}</strong>", query);
            }
            else
            {
                message.Append("We found some content matching these search terms: ");
                message.AppendFormat("<strong>{0}</strong>", query);
                message.Append("<br/>");
                message.Append("<br/>");
                message.Append("<ul>");
                foreach (var result in results)
                {
                    message.Append("<li>");
                    message.AppendFormat("<strong>Id</strong>: {0}, <strong>Name</strong>: {1}, <strong>Url</strong>: {2}", result.Id, result.Fields["nodeName"], result.Fields["urlName"]);
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

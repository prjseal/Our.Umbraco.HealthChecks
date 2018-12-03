using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Config
{
    [HealthCheck("4e5064b2-e0d1-4945-9a26-f42026080902", "XML Sitemap",
    Description = "Look for a sitemap at the address /sitemap.xml or look in the robots.txt file for any sitemaps and check that those exist",
    Group = "SEO")]
    public class XmlSitemapCheck : HealthCheck
    {
        protected readonly ILocalizedTextService TextService;
        protected readonly string BaseUrl;
        protected bool CheckSitemapUrlStatus;
        protected int RobotSitemaps;
        protected int RobotSitemapsChecked;
        
        public XmlSitemapCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            TextService = healthCheckContext.ApplicationContext.Services.TextService;
            BaseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckXmlSitemap(), CheckRobotsSitemaps() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("XmlSitemapCheck has no executable actions");
        }

        private HealthCheckStatus CheckXmlSitemap()
        {
            StringBuilder message = new StringBuilder();
            
            // Check if there is a default sitemap at /sitemap.xml
            message = CheckSitemapUrl(message, BaseUrl + "/sitemap.xml", true).Result;

            var actions = new List<HealthCheckAction>();

            var success = CheckSitemapUrlStatus;

            return
                new HealthCheckStatus(message.ToString())
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus CheckRobotsSitemaps()
        {
            StringBuilder message = new StringBuilder();

            // Check if robots.txt exists in the root of the website, if it does, check any sitemaps mentioned
            message = ProcessRobotsFile(message).Result;

            var actions = new List<HealthCheckAction>();

            var success = RobotSitemaps == RobotSitemapsChecked;

            return
                new HealthCheckStatus(message.ToString())
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private async Task<StringBuilder> CheckSitemapUrl(StringBuilder message, string url, bool defaultSitemap)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);
                var msg = defaultSitemap ? TextService.Localize("Our.Umbraco.HealthChecks/sitemapDefaultMessage") : string.Format("{0} {1}", TextService.Localize("Our.Umbraco.HealthChecks/sitemapDefaultMessage"), url);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        if (defaultSitemap)
                        {
                            CheckSitemapUrlStatus = true;
                        }
                        else
                        {
                            RobotSitemapsChecked++;
                        }
                        message.Append(string.Format("{0} {1}", msg, TextService.Localize("Our.Umbraco.HealthChecks/sitemapFound")));
                        message.Append("<br/>");
                        break;
                    case HttpStatusCode.NotFound:
                        message.Append(string.Format("{0} {1}", msg, TextService.Localize("Our.Umbraco.HealthChecks/sitemapNotFound")));
                        message.Append("<br/>");
                        break;
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.BadRequest:
                        message.Append(string.Format("{0} {1}", TextService.Localize("Our.Umbraco.HealthChecks/sitemapError"), msg));
                        message.Append("<br/>");
                        break;
                }
            }

            return message;
        }

        private async Task<StringBuilder> ProcessRobotsFile(StringBuilder message)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(BaseUrl + "/robots.txt").ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(content))
                    {
                        message.Append(TextService.Localize("Our.Umbraco.HealthChecks/robotsEmpty"));
                        message.Append("<br/>");
                    }
                    else
                    {
                        string[] lines = content
                            .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToArray();
                        if (lines.Length > 0)
                        {
                            var sitemaps = GetSitemapLines(lines);
                            if (sitemaps.Any())
                            {
                                foreach (var sitemap in sitemaps)
                                {
                                    RobotSitemaps++;
                                    message = CheckSitemapUrl(message, sitemap, false).Result;
                                }
                            }
                            else
                            {
                                message.Append(TextService.Localize("Our.Umbraco.HealthChecks/robotsContainsNoSitemaps"));
                                message.Append("<br/>");
                            }
                        }
                        else
                        {
                            message.Append(TextService.Localize("Our.Umbraco.HealthChecks/robotsEmpty"));
                            message.Append("<br/>");
                        }
                    }
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            message.Append(TextService.Localize("Our.Umbraco.HealthChecks/robotsNotFound"));
                            break;
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.BadRequest:
                            message.Append(TextService.Localize("Our.Umbraco.HealthChecks/robotsError"));
                            break;
                    }
                    // Make the count less than RobotSitemaps to make check fail
                    RobotSitemapsChecked--;
                    message.Append("<br/>");
                }
            }

            return message;
        }

        private List<string> GetSitemapLines(string[] lines)
        {
            var sitemaps = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.StartsWith("#"))
                {
                    // whole line is comment
                    continue;
                }
                // if line contains comments, get rid of them
                if (line.IndexOf('#') > 0)
                {
                    line = line.Remove(line.IndexOf('#'));
                }

                line = line.Trim();
                var index = line.IndexOf(':');
                if (index == -1)
                {
                    continue;
                }

                string field = GetField(line);
                if (string.IsNullOrWhiteSpace(field))
                {
                    // If could not find the first ':' char or if there wasn't a field declaration before ':'
                    continue;
                }

                if (field.InvariantEquals("sitemap"))
                {
                    sitemaps.Add(line.Substring(field.Length + 1).Trim());
                }
            }

            return sitemaps;
        }

        private string GetField(string line)
        {
            var index = line.IndexOf(':');
            if (index == -1)
            {
                return string.Empty;
            }

            return line.Substring(0, index);
        }
    }
}
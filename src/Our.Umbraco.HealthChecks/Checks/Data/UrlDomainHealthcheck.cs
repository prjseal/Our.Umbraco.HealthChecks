using Our.Umbraco.Healthchecks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using System.Configuration;
using Umbraco.Web.HealthCheck;
using Umbraco.Core.Persistence;

namespace Our.Umbraco.Healthchecks
{
    [HealthCheck(
          "de366476-8e6e-4c72-9b29-71294df9d7e3",
          "Pre-Production Url Picker Domains",
          Description = "Checks the database for hardcoded references to 'pre-production' domains in Url Picked properties. Defaults to look for localhost, configure domains to look for via appsetting: Our.Umbraco.Healthchecks.UrlPickerDomains",
          Group = "Data")]
    public class UrlPickerDomainsHealthCheck : HealthCheck
    {
        private readonly UmbracoDatabase _db;


        public UrlPickerDomainsHealthCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _db = healthCheckContext.ApplicationContext.DatabaseContext.Database;
        }
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            var statusesToCheck = new List<HealthCheckStatus>();
            //TODO: should default to localhost
            var domainListToCheck = "localhost";
            if (ConfigurationManager.AppSettings.AllKeys.Contains("Our.Umbraco.Healthchecks.UrlPickerDomains"))
            {
                domainListToCheck = ConfigurationManager.AppSettings["Our.Umbraco.Healthchecks.UrlPickerDomains"];
            }
            //check for multiple domains
            var domainsToCheck = domainListToCheck.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //read in config value split on comma
            //foreach adn add checkfordomain for each one.
            foreach (var domain in domainsToCheck) {
                statusesToCheck.Add(CheckForDomain(domain));
            }
            return statusesToCheck;
        }
        private string HighlightMatch(string data, string domain)
        {
            string snippet = String.Empty;
            if (String.IsNullOrEmpty(data))
            {
                return String.Empty;
            }
            //TODO: what if the domain is mentioned more than once in the string?

            var matchPosition = data.IndexOf(domain);
            if (matchPosition > -1)
            {
                var startSnippetPos = matchPosition > 30 ? matchPosition - 30 : 0;
                var remainingData = data.Length - matchPosition - domain.Length;
                var snippetLength = remainingData <= 30 ? domain.Length : domain.Length + 60;
                snippet = data.Substring(startSnippetPos, snippetLength);
            }
            return snippet.Replace(domain, "<b>" + domain + "</b>");

        }
        private HealthCheckStatus CheckForDomain(string domain)
        {
            StringBuilder resultMessage = new StringBuilder();
            StatusResultType resultType = StatusResultType.Info;
            var actions = new List<HealthCheckAction>();
            var propertyDataEntries = _db.Query<PropertyDataDetails>("SELECT umbracoNode.id, umbracoNode.[Text], cmsPropertyType.alias, published, updateDate, dataNvarchar, dataNText FROM cmsPropertyData INNER JOIN cmsPropertyType On cmsPropertyType.Id = cmsPropertyData.propertytypeid INNER JOIN UmbracoNode on umbracoNode.Id = cmsPropertyData.contentNodeId INNER JOIN cmsDocument on cmsDocument.versionID = cmsPropertyData.versionId WHERE (dataNtext is not null or dataNvarchar is not null) AND (dataNtext like @0 OR dataNvarchar LIKE @0)", "%" + domain + "%");
            //var propertyDataEntries = _db.Query<PropertyDataDetails>("SELECT umbracoNode.id, umbracoNode.[Text], cmsPropertyType.alias, published, updateDate, dataNvarchar, dataNText FROM cmsPropertyData INNER JOIN cmsPropertyType On cmsPropertyType.Id = cmsPropertyData.propertytypeid INNER JOIN UmbracoNode on umbracoNode.Id = cmsPropertyData.contentNodeId INNER JOIN cmsDocument on cmsDocument.versionID = cmsPropertyData.versionId WHERE (dataNtext is not null or dataNvarchar is not null) AND (dataNtext like '%moriyama%' OR dataNvarchar LIKE '%moriyama%')");
            if (propertyDataEntries != null && propertyDataEntries.Any())
            {
                resultType = StatusResultType.Error;
                resultMessage.AppendLine("There are " + propertyDataEntries.Count() + " references to '" + domain + "' in property data");
                resultMessage.AppendLine("<p><a href='/umbraco/backoffice/api/UrlDomainReport/PropertyDataInstances?domain=" + domain + "'>Full Report</a>: /umbraco/backoffice/api/UrlDomainReport/PropertyDataInstances?domain=" + domain + "</p>");
                resultMessage.AppendLine("<table>");
                resultMessage.AppendLine("<tr><th>id</th><th>Name</th><th>Alias</th><th>published</th><th>updated</th><th>Data</th></tr>");
                foreach (var propertyDataEntry in propertyDataEntries.Where(f => f.published).OrderByDescending(f => f.updateDate).Take(20))
                {
                    var data = HighlightMatch(propertyDataEntry.dataNText, domain) + HighlightMatch(propertyDataEntry.dataNvarchar, domain);
                    resultMessage.AppendLine($"<tr><td><a href='/umbraco#/content/content/edit/{propertyDataEntry.id}'>{propertyDataEntry.id}</a></td><td>{propertyDataEntry.text}</td><td>{propertyDataEntry.alias}</td><td>{propertyDataEntry.published}</td><td>{propertyDataEntry.updateDate:dd/MM/yyyy hh:mm}</td><td>{propertyDataEntry.id}</td><td>{data}</td></tr>");
                    // resultMessage.AppendLine("<br />" + propertyDataEntry.id.ToString());
                }
                resultMessage.AppendLine("</table>");
              
                // ng-safe-html attribute is stripping out the href!
                resultMessage.AppendLine("<p><a href='/umbraco/backoffice/api/UrlDomainReport/PropertyDataInstances?domain=" + domain +"'>Full Report</a>: /umbraco/backoffice/api/UrlDomainReport/PropertyDataInstances?domain=" + domain + "</p>");

                // so I tried adding an action... but didn't get far in making the download occur when requesting the api
                //actions.Add(new HealthCheckAction("downloadPropertyDataReport", Id)
                //// Override the "Rectify" button name and describe what this action will do
                //{
                //    Name = "Full Report",
                //    Description = "Download full list of PropertyDataDetails",
                //    ActionParameters = new Dictionary<string, object>()
                //         {
                //             { "domain",domain }
                //         },
                //    ProvidedValue = domain
                //});
                resultMessage.AppendLine("<p>Useful SQL - set hardcoded domains to be relative links:</p>");
                resultMessage.AppendLine("<p>");
                resultMessage.AppendLine("UPDATE cmsPropertyData ");
                resultMessage.AppendLine("SET dataNtext = CAST(replace(CAST(dataNtext as NVarchar(MAX)), 'https://yourdomain.com/', '/') as NText),");
                resultMessage.AppendLine("dataNvarchar = replace(dataNvarchar, 'https://yourdomain.com/', '/')");
                resultMessage.AppendLine("WHERE");
                resultMessage.AppendLine("(dataNtext is not null or dataNvarchar is not null)");
                resultMessage.AppendLine("AND");
                resultMessage.AppendLine("(dataNtext like '%https://yourdomain.com/%'");
                resultMessage.AppendLine("OR dataNvarchar like '%https://yourdomain.com/%')");
                resultMessage.AppendLine("</p>");
            }
            else
            {
                resultType = StatusResultType.Success;
                resultMessage.AppendLine("<p>There are no references to " + domain + " in property data</p>");
            }
            return new HealthCheckStatus(resultMessage.ToString())
            {
                ResultType = resultType,
                Actions = actions
            };
        }
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            // NO ACTIONS - Tried to get an action to download the report but failed!
            switch (action.Alias)
            {
                case "downloadPropertyDataReport":
                    // download the report and output to the response?
                    //umbraco/backoffice/api/UrlDomainReport/PropertyDataInstances?domain=" + action.ProvidedValue);
                    //using (var client = new WebClient())
                    //{
                    //    string domain = action.ProvidedValue.ToUrlSegment();
                    //    string filename = $"{DateTime.Now:yy-MM-dd}_{domain}_PropertyDataInstances.html";
                    //    client.DownloadFile("/umbraco/backoffice/api/UrlDomainReport/PropertyDataInstances?domain=" + action.ProvidedValue),filename);
                    //}
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var message = new StringBuilder();
            message.AppendLine("<p>Useful SQL - set hardcoded domains to be relative links:</p>");
            message.AppendLine("<p>");
            message.AppendLine("UPDATE cmsPropertyData ");
            message.AppendLine("SET dataNtext = CAST(replace(CAST(dataNtext as NVarchar(MAX)), 'https://yourdomain.com/', '/') as NText),");
            message.AppendLine("dataNvarchar = replace(dataNvarchar, 'https://yourdomain.com/', '/')");
            message.AppendLine("WHERE");
            message.AppendLine("(dataNtext is not null or dataNvarchar is not null)");
            message.AppendLine("AND");
            message.AppendLine("(dataNtext like '%https://yourdomain.com/%'");
            message.AppendLine("OR dataNvarchar like '%https://yourdomain.com/%')");
            message.AppendLine("</p>");
            return
                new HealthCheckStatus(message.ToString())
                {
                    ResultType = StatusResultType.Warning,
                    Actions = new List<HealthCheckAction>()
        };

        }
    }

}

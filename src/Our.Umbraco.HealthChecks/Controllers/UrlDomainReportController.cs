using Our.Umbraco.Healthchecks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.WebApi;

namespace Our.Umbraco.Healthchecks.Controllers
{
    public class UrlDomainReportController : UmbracoAuthorizedApiController
    {
        [HttpGet]
        public HttpResponseMessage PropertyDataInstances(string domain)
        {
            var propertyDataEntries = ApplicationContext.DatabaseContext.Database.Query<PropertyDataDetails>("SELECT umbracoNode.id, umbracoNode.[Text], cmsPropertyType.alias, published, updateDate, dataNvarchar, dataNText FROM cmsPropertyData INNER JOIN cmsPropertyType On cmsPropertyType.Id = cmsPropertyData.propertytypeid INNER JOIN UmbracoNode on umbracoNode.Id = cmsPropertyData.contentNodeId INNER JOIN cmsDocument on cmsDocument.versionID = cmsPropertyData.versionId WHERE (dataNtext is not null or dataNvarchar is not null) AND (dataNtext like @0 OR dataNvarchar LIKE @0)", "%" + domain + "%");

            StringBuilder resultMessage = new StringBuilder();
            resultMessage.AppendLine("<html><head><title>" + domain + "</title></head><body>");
            if (propertyDataEntries != null && propertyDataEntries.Any())
            {

                resultMessage.AppendLine("There are " + propertyDataEntries.Count() + " references to '" + domain + "' in property data");
                resultMessage.AppendLine("<table>");
                resultMessage.AppendLine("<tr><th>id</th><th>Name</th><th>Alias</th><th>published</th><th>updated</th><th>Data</th></tr>");
                foreach (var propertyDataEntry in propertyDataEntries.Where(f => f.published).OrderByDescending(f => f.updateDate).Take(50))
                {
                    resultMessage.AppendLine($"<tr><td>{propertyDataEntry.id}</td><td>{propertyDataEntry.text}</td><td>{propertyDataEntry.alias}</td><td>{propertyDataEntry.published}</td><td>{propertyDataEntry.updateDate:dd/MM/yyyy hh:mm}</td><td>{propertyDataEntry.id}</td><td><textarea>{propertyDataEntry.dataNvarchar}{propertyDataEntry.dataNText}</textarea></td></tr>");
                    // resultMessage.AppendLine("<br />" + propertyDataEntry.id.ToString());
                }
                resultMessage.AppendLine("</table>");

            }
            else
            {
                resultMessage.AppendLine("<p>There are no references to " + domain + " in property data</p>");
            }
            resultMessage.AppendLine("</body></html>");
         
            string filename = $"{DateTime.Now:yy-MM-dd}_{domain.ToUrlSegment()}_PropertyDataInstances.html";

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

            result.Content = new StringContent(resultMessage.ToString());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = filename
            };
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Config
{
    [HealthCheck("CA765D50-85D9-4346-BBC4-8DEEBB7EBAE2", "PostProcessor Check",
    Description = "Check if ImageProcessor.Web.PostProcessor is installed",
    Group = "Configuration")]
    public class PostProcessorCheck : HealthCheck
    {
        protected readonly ILocalizedTextService TextService;
        protected readonly HttpServerUtilityBase Server;
        private const string FilePath = "~/packages.config";
        private const string XPath = "//packages/package[@id='ImageProcessor.Web.PostProcessor']";

        public PostProcessorCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            TextService = healthCheckContext.ApplicationContext.Services.TextService;
            Server = healthCheckContext.HttpContext.Server;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckForPostProcessorIsInstalled() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("PostProcessorCheck has no executable actions");
        }

        private HealthCheckStatus CheckForPostProcessorIsInstalled()
        {
            StringBuilder message = new StringBuilder();
            var success = false;




            if(File.Exists(Server.MapPath(FilePath)))
            {
                var absoluteFilePath = IOHelper.MapPath(FilePath);
                var xmlDocument = new XmlDocument { PreserveWhitespace = true };
                xmlDocument.Load(absoluteFilePath);
                var xmlNode = xmlDocument.SelectSingleNode(XPath);
                if (xmlNode != null)
                {
                    success = true;
                    message.Append(TextService.Localize("Our.Umbraco.HealthChecks/postProcessorSuccess"));
                }
            }

            if(!success)
            {
                message.Append(TextService.Localize("Our.Umbraco.HealthChecks/postProcessorError"));
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Config
{
    [HealthCheck("CA765D50-85D9-4346-BBC4-8DEEBB7EBAE2", "PostProcessor Check - (from Our.Umbraco.HealthChecks)",
    Description = "Check if ImageProcessor.Web.PostProcessor is installed",
    Group = "Configuration")]
    public class PostProcessorCheck : HealthCheck
    {
        protected readonly ILocalizedTextService _textService;
        private const string FilePath = "~/packages.config";
        private const string XPath = "//packages/package[@id='ImageProcessor.Web.PostProcessor']";

        public PostProcessorCheck(ILocalizedTextService textService)
        {
            _textService = textService;
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
            var absoluteFilePath = IOHelper.MapPath(FilePath);

            if (File.Exists(absoluteFilePath))
            {
                var xmlDocument = new XmlDocument { PreserveWhitespace = true };
                xmlDocument.Load(absoluteFilePath);
                var xmlNode = xmlDocument.SelectSingleNode(XPath);
                if (xmlNode != null)
                {
                    success = true;
                    message.Append(_textService.Localize("Our.Umbraco.HealthChecks/postProcessorSuccess"));
                }
            }

            if(!success)
            {
                message.Append(_textService.Localize("Our.Umbraco.HealthChecks/postProcessorError"));
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
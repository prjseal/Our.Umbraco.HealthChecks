using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Our.Umbraco.HealthChecks.Checks.Services
{
    [HealthCheck("4e5064b2-e0d1-4945-9a26-f42026080701", "Examine Rebuild On Startup",
    Description = "Check whether examine rebuild on start is off",
    Group = "Configuration")]
    public class ExamineRebuildOnStartupCheck : HealthCheck
    {
        protected readonly ILocalizedTextService TextService;
        private const string SetExamineConfigAction = "setExamineConfig";
        private const string FilePath = "~/config/examineSettings.config";
        private const string XPath = "/Examine/@RebuildOnAppStart";
        private const string Attribute = "RebuildOnAppStart";

        public ExamineRebuildOnStartupCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            TextService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckExamineRebuildOnStartup() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case SetExamineConfigAction:
                    return SetExamineConfig();
                default:
                    throw new InvalidOperationException("Examine Rebuild On Startup action requested is either not executable or does not exist");
            }
        }

        private HealthCheckStatus CheckExamineRebuildOnStartup()
        {
            StringBuilder message = new StringBuilder();
            var success = false;

            var absoluteFilePath = IOHelper.MapPath(FilePath);
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };
            xmlDocument.Load(absoluteFilePath);

            var xmlNode = xmlDocument.SelectSingleNode(XPath);
            if (xmlNode != null && xmlNode.InnerText.InvariantEquals("false"))
            {
                success = true;
                message.Append(TextService.Localize("Our.Umbraco.HealthChecks/examineRebuildSuccess"));
            }
            else
            {
                message.Append(TextService.Localize("Our.Umbraco.HealthChecks/examineRebuildError"));
            }

            var actions = new List<HealthCheckAction>();
            if (success == false)
            {
                actions.Add(new HealthCheckAction(SetExamineConfigAction, Id)
                {
                    Name = TextService.Localize("Our.Umbraco.HealthChecks/setExamineConfig"),
                    Description = TextService.Localize("Our.Umbraco.HealthChecks/setExamineConfigDescription")
                });
            }

            return
                new HealthCheckStatus(message.ToString())
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus SetExamineConfig()
        {
            var errorMessage = string.Empty;
            var success = SaveExamineConfigFile(out errorMessage);

            if (success)
            {
                return
                    new HealthCheckStatus(TextService.Localize("Our.Umbraco.HealthChecks/setExamineConfigSuccess"))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(TextService.Localize("Our.Umbraco.HealthChecks/setExamineConfigError", new[] { errorMessage }))
                {
                    ResultType = StatusResultType.Error
                };
        }

        private bool SaveExamineConfigFile(out string errorMessage)
        {
            try
            {
                var absoluteFilePath = IOHelper.MapPath(FilePath);
                if (File.Exists(absoluteFilePath))
                {
                    var doc = XDocument.Load(absoluteFilePath, LoadOptions.PreserveWhitespace);
                    var examineElement = doc.XPathSelectElement("/Examine");

                    if (examineElement != null && examineElement.Attribute(Attribute) != null)
                    {
                        examineElement.Attribute(Attribute).Value = "false";
                    }
                    else if (examineElement != null && examineElement.Attribute(Attribute) == null)
                    {
                        examineElement.Add(new XAttribute(Attribute, "false"));
                    }

                    doc.Save(absoluteFilePath, SaveOptions.DisableFormatting);

                    errorMessage = string.Empty;
                    return true;
                }
                errorMessage = string.Format("File not found at {0}", FilePath);
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
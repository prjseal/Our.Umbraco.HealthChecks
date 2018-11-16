using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Our.Umbraco.HealthChecks.Checks.Config
{
    [HealthCheck("4e5064b2-e0d1-4945-9a26-f42026080701", "Examine Rebuild On Startup",
    Description = "Check whether examine rebuild on start is off",
    Group = "Configuration")]
    public class ExamineRebuildOnStartupCheck : HealthCheck
    {
        protected readonly ILocalizedTextService TextService;
        private const string SetExamineConfigAction = "setExamineConfig";
        private ConfigurationService _configurationService;
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
            
            _configurationService = new ConfigurationService(absoluteFilePath, XPath);
            var configValue = _configurationService.GetConfigurationValue();
            
            if (configValue.Success && configValue.Result.InvariantEquals("false"))
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
                // There don't look to be any useful classes defined in https://msdn.microsoft.com/en-us/library/system.web.configuration(v=vs.110).aspx
                // for working with the customHeaders section, so working with the XML directly.
                var absoluteFilePath = IOHelper.MapPath(FilePath);
                _configurationService = new ConfigurationService(absoluteFilePath, XPath);
                
                var configFile = IOHelper.MapPath(FilePath);
                var doc = XDocument.Load(configFile);
                var examineElement = doc.XPathSelectElement("/Examine");

                if (examineElement != null && examineElement.Attribute(Attribute) != null)
                {
                    examineElement.Attribute(Attribute).Value = "false";
                }
                else if (examineElement != null && examineElement.Attribute(Attribute) == null)
                {
                    examineElement.Add(new XAttribute(Attribute, "false"));
                }
                
                doc.Save(configFile);

                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
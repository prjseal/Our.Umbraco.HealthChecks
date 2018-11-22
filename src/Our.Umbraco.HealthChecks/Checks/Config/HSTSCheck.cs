using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Config
{
    [HealthCheck("6437384C-D1D3-46DA-9E21-9E0BC1498E1F", "HSTS Check (from Our.Umbraco.HealthChecks)",
    Description = "Check to see if the HSTS policy is set on the website.",
    Group = "Security")]
    public class HstsCheck : HealthCheck
    {
        protected readonly ILocalizedTextService TextService;
        private const string SetHstsConfigAction = "setHstsConfig";
        private const string FilePath = "~/web.config";
        private const string XPath = "/configuration/system.webServer/rewrite/outboundRules/rule/match[@serverVariable='RESPONSE_Strict_Transport_Security']";
        private const string Attribute = "enabled";

        public HstsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            TextService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckHSTS() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case SetHstsConfigAction:
                    return SetHstsConfig();
                default:
                    throw new InvalidOperationException("HSTS action requested is either not executable or does not exist");
            }
        }

        private HealthCheckStatus CheckHSTS()
        {
            StringBuilder message = new StringBuilder();
            var success = false;

            var absoluteFilePath = IOHelper.MapPath(FilePath);
            if (File.Exists(absoluteFilePath))
            {
                var xmlDocument = XDocument.Load(absoluteFilePath, LoadOptions.PreserveWhitespace);
                var hstsElement = xmlDocument.XPathSelectElement(XPath);

                if (hstsElement != null)
                {
                    success = true;
                    message.Append(TextService.Localize("Our.Umbraco.HealthChecks/hstsSuccess"));
                }
            }

            if(!success)
            {
                message.Append(TextService.Localize("Our.Umbraco.HealthChecks/hstsError"));
            }

            var actions = new List<HealthCheckAction>();
            if (success == false)
            {
                actions.Add(new HealthCheckAction(SetHstsConfigAction, Id)
                {
                    Name = TextService.Localize("Our.Umbraco.HealthChecks/setHstsConfig"),
                    Description = TextService.Localize("Our.Umbraco.HealthChecks/setHstsConfigDescription")
                });
            }

            return
                new HealthCheckStatus(message.ToString())
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus SetHstsConfig()
        {
            var errorMessage = string.Empty;
            var success = SaveWebConfigFile(out errorMessage);

            if (success)
            {
                return
                    new HealthCheckStatus(TextService.Localize("Our.Umbraco.HealthChecks/setHstsConfigSuccess"))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(TextService.Localize("Our.Umbraco.HealthChecks/setHstsConfigError", new[] { errorMessage }))
                {
                    ResultType = StatusResultType.Error
                };
        }

        private bool SaveWebConfigFile(out string errorMessage)
        {
            try
            {
                var absoluteFilePath = IOHelper.MapPath(FilePath);
                if (File.Exists(absoluteFilePath))
                {
                    var doc = XDocument.Load(absoluteFilePath, LoadOptions.PreserveWhitespace);
                    var systemWebServerElement = doc.XPathSelectElement("/configuration/system.webServer");

                    if(systemWebServerElement != null)
                    {
                        //Element set up as per Scott Hanselman's blog post.
                        //https://www.hanselman.com/blog/HowToEnableHTTPStrictTransportSecurityHSTSInIIS7.aspx#highlighter_5478

                        XElement hstsRuleElement = new XElement("rule",
                            new XAttribute("name", "Add Strict-Transport-Security when HTTPS"),
                            new XAttribute("enabled", "true"),
                            new XElement("match",
                                new XAttribute("serverVariable", "RESPONSE_Strict_Transport_Security"),
                                new XAttribute("pattern", ".*")),
                            new XElement("conditions",
                                new XElement("add",
                                    new XAttribute("input", "{HTTPS}"),
                                    new XAttribute("pattern", "on"),
                                    new XAttribute("ignoreCase", "true"))),
                            new XElement("action",
                                new XAttribute("type", "Rewrite"),
                                new XAttribute("value", "max-age=31536000; preload")));

                        var rewriteElement = doc.XPathSelectElement("/configuration/system.webServer/rewrite");

                        if(rewriteElement != null)
                        {
                            var outboundRulesElement = doc.XPathSelectElement("/configuration/system.webServer/rewrite/outboundRules");

                            if(outboundRulesElement != null)
                            {
                                outboundRulesElement.Add(hstsRuleElement);
                            }
                            else
                            {
                                rewriteElement.Add(new XElement("outboundRules", hstsRuleElement));
                            }
                        }
                        else
                        {
                            systemWebServerElement.Add(new XElement("rewrite", new XElement("outboundRules", hstsRuleElement)));
                        }
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
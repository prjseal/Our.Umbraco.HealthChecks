using System;
using System.Collections.Generic;
using System.Net;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Security
{
    [HealthCheck("92AE66E1-209D-4F9E-AAF5-19B19D41CF49", "TLS Check",
        Description = "Check the TLS protocol being used",
        Group = "Security")]
    public class TlsCheck : HealthCheck
    {
        protected readonly ILocalizedTextService TextService;

        public TlsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            TextService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckTls() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("TlsCheck has no executable actions");
        }

        public HealthCheckStatus CheckTls()
        {
            bool success = (int)ServicePointManager.SecurityProtocol >= (int)SecurityProtocolType.Tls12;

            string message = success ? TextService.Localize("Our.Umbraco.HealthChecks/tlsCheckSuccess") : TextService.Localize("Our.Umbraco.HealthChecks/tlsCheckError");

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }
    }
}

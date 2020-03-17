using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.Security
{
    [HealthCheck("C6D425DF-47A6-4526-A915-AAA39192634D", "Client Depency Version Check - (from Our.Umbraco.HealthChecks)",
        Description = "Check the version number of ClientDepency.Core.dll",
        Group = "Security")]
    public class ClientDependencyVersionCheck : HealthCheck
    {
        protected readonly ILocalizedTextService _textService;
        private const string DirectoryPath = "~/bin/";
        private const string DllName = "ClientDependency.Core.dll";
        private const string MinimumVersionNumber = "1.9.9";

        public ClientDependencyVersionCheck(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckClientDependencyVersion() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("ClientDependencyVersionCheck has no executable actions");
        }

        public HealthCheckStatus CheckClientDependencyVersion()
        {
            var directory = IOHelper.MapPath(DirectoryPath);
            var filePath = Path.Combine(directory, DllName);
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);

            var success = GreaterThanOrEqualToMinimumVersion(fileVersionInfo.FileVersion, MinimumVersionNumber);

            string message = success ? _textService.Localize("Our.Umbraco.HealthChecks/clientDependencyVersionCheckSuccess") : _textService.Localize("Our.Umbraco.HealthChecks/clientDependencyVersionCheckError");

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private bool GreaterThanOrEqualToMinimumVersion(string versionNumber, string minimumVersionNumber)
        {
            if (versionNumber == minimumVersionNumber || string.IsNullOrEmpty(minimumVersionNumber))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(versionNumber))
            {
                int[] versionNumberSet = versionNumber.Split('.').Select(x => int.Parse(x)).ToArray();
                int[] minimumVersionNumberSet = minimumVersionNumber.Split('.').Select(x => int.Parse(x)).ToArray();

                int versionNumberSetDepth = versionNumberSet.Length;
                int minimumVersionNumberSetDepth = minimumVersionNumberSet.Length;

                int versionDepth = Math.Min(versionNumberSetDepth, minimumVersionNumberSetDepth);

                for (int depth = 0; depth < versionDepth; depth++)
                {
                    if (versionNumberSet[depth] > minimumVersionNumberSet[depth])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
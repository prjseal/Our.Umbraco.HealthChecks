using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Our.Umbraco.HealthChecks.Checks.Azure
{
    [HealthCheck("35631050-103B-45A9-AE24-EDF2E1E82DA6", "Azure Examine Compatibility Check - (from Our.Umbraco.HealthChecks)",
        Description = "Checks that examine settings are appropriate for the Azure platform.",
        Group = "Azure")]
    public class AzureExamineCheck : AbstractConfigCheck
    {
        public AzureExamineCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {

        }

        public override string FilePath
        {
            get { return "~/config/ExamineSettings.config"; }
        }

        public override string XPath
        {
            get { return "/Examine/ExamineIndexProviders/providers/add/@directoryFactory"; }
        }

        public override bool CanRectify
        {
            get { return false; }
        }

        public override ValueComparisonType ValueComparisonType
        {
            get { return ValueComparisonType.ShouldEqual; }
        }

        public override IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                return new List<AcceptableConfiguration>
                {
                    new AcceptableConfiguration { IsRecommended = true, Value = "Examine.LuceneEngine.Directories.TempEnvDirectoryFactory,Examine" },
                    new AcceptableConfiguration { IsRecommended = true, Value = "Examine.LuceneEngine.Directories.TempEnvDirectoryFactory, Examine" },
                    new AcceptableConfiguration { IsRecommended = true, Value = "Examine.LuceneEngine.Directories.SyncTempEnvDirectoryFactory,Examine" },
                    new AcceptableConfiguration { IsRecommended = true, Value = "Examine.LuceneEngine.Directories.SyncTempEnvDirectoryFactory, Examine" }
                };
            }
        } 

        public override string CheckSuccessMessage
        {
            get { return $"Examine directory factory is set to '{CurrentValue}'"; }
        }

        public override string CheckErrorMessage
        {
            get { return $"Examine directory factory should be set to '{Values.First(v => v.IsRecommended).Value}', but is currently set to '{CurrentValue}'"; }
        }
    }
}

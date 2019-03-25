using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.HealthChecks.Checks.DataIntegrity
{
    [HealthCheck("477bbf21-5d34-4af9-a304-0b44c3ed4dea", "Content Versions", Description = "Checks average number of historical 'versions' kept for each content item in your site, too many versions can slow the Umbraco backoffice", Group = "Data Integrity")]
    public class ContentVersionsCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly UmbracoDatabase _db;
        public ContentVersionsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            //ToDo: perhaps should localize the text messages
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
            _db = healthCheckContext.ApplicationContext.DatabaseContext.Database;
        }
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //We may need to tweak these values to get the healtcheck more helpful
            const int NO_OF_VERSIONS_PER_SINGLECONTENTITEM_IS_TOO_MANY = 10;
            const int NO_OF_VERSIONS_PER_SITE_IS_TOO_MANY = 1000000;
            const int AVERAGE_NO_OF_VERSIONS_PER_CONTENTITEM_IS_TOO_HIGH = 5;
            var statusesToCheck = new List<HealthCheckStatus>();

            //pass whether UnVersion installed into the versioning healthchecks to vary the messages/advice accordingly
            bool isUnversionInstalled = UnVersionInstalled();
            statusesToCheck.Add(CheckUnversionInstalled(isUnversionInstalled));
            statusesToCheck.Add(CheckTotalNumberOfVersions(isUnversionInstalled, NO_OF_VERSIONS_PER_SITE_IS_TOO_MANY));
            statusesToCheck.Add(CheckAverageNumberOfVersionsPerContent(isUnversionInstalled, AVERAGE_NO_OF_VERSIONS_PER_CONTENTITEM_IS_TOO_HIGH));
            statusesToCheck.Add(CheckMostVersionedContent(isUnversionInstalled, NO_OF_VERSIONS_PER_SINGLECONTENTITEM_IS_TOO_MANY));
            return statusesToCheck;
        }
        #region Discovery
        private decimal GetAverageVersionsPerContentItem()
        {
            // is this skewed if there are lots of entries with a single version?
            return _db.ExecuteScalar<decimal>("Select avg(CAST(VersionCount AS FLOAT)) as AverageVersionsPerDoc from (SELECT Count(ContentId) as VersionCount FROM cmsContentVersion Group By ContentId) as VersionCounts");

        }
        private IEnumerable<VersionInfo> GetContentWithMoreVersionsThan(int versionCount = 10)
        {

            return _db.Query<VersionInfo>("SELECT [ContentId], [text], Count(ContentId) as VersionCount FROM cmsContentVersion INNER JOIN umbracoNode ON cmsContentVersion.ContentId = umbracoNode.id  GROUP BY [ContentId], [text] HAVING COUNT(ContentId) > @0 ORDER BY VersionCount DESC", versionCount);

        }
        private int TotalNumberOfVersions()
        {
            return _db.ExecuteScalar<int>("Select Count(*) from cmsContentVersion");
        }
        private bool UnVersionInstalled()
        {
            // determine if UnVersion package is installed
            return AppDomain.CurrentDomain.GetAssemblies().Select(f => f.FullName.Split(',')[0]).Any(f => f == "Our.Umbraco.UnVersion");
        }
        #endregion
        #region HealthChecks

        private HealthCheckStatus CheckUnversionInstalled(bool isUnversionInstalled)
        {
            StatusResultType resultType = StatusResultType.Info;

            var message = "Our.Umbraco.UnVersion is installed, see /config/Unversion.config for versioning policy settings";
            if (!isUnversionInstalled)
            {
                resultType = StatusResultType.Warning;
                message = "'Our.Umbraco.UnVersion' is NOT installed, consider installing this package to help create a version history retainment policy.";
            }

            var actions = new List<HealthCheckAction>();
            return
               new HealthCheckStatus(message)
               {
                   ResultType = resultType,
                   Actions = actions
               };
        }
        private HealthCheckStatus CheckTotalNumberOfVersions(bool IsUnversionInstalled, int warningLevelOfVersions)
        {
            int totalNumberOfVersions = TotalNumberOfVersions();
            StatusResultType resultType = StatusResultType.Info;
            var message = String.Format("Total number of content versions: {0} ", totalNumberOfVersions.ToString());
            if (totalNumberOfVersions > warningLevelOfVersions)
            {
                resultType = StatusResultType.Warning;
                message = message + String.Format(" - You have a over a million content versions on your Umbraco site, the backoffice may begin to slow down and you may want to consider [Installing Our.Umbraco.Unversion/Updating your Our.Umbraco.Unversion configuration] or manually removing older content versions from the database", IsUnversionInstalled ? "Updating your 'Our.Umbraco.UnVersion' configuration" : "Installing 'Our.Umbraco.UnVersion'") + " or manually removing older content versions from the database.";
            }

            var actions = new List<HealthCheckAction>();
            return
               new HealthCheckStatus(message)
               {
                   ResultType = resultType,
                   Actions = actions
               };
        }
        private HealthCheckStatus CheckAverageNumberOfVersionsPerContent(bool IsUnversionInstalled, int warningLevelForAverage)
        {

            StatusResultType resultType = StatusResultType.Info;
            decimal averageNoOfVersionsPerContentItem = GetAverageVersionsPerContentItem();
            var message = String.Format("Average number of versions per content item: {0} ", averageNoOfVersionsPerContentItem.ToString("F2"));
            if (averageNoOfVersionsPerContentItem > warningLevelForAverage)
            {
                resultType = StatusResultType.Warning;
                message = message + " - " + String.Format("On average it appears you have a large amount of old content versions for each published content item, this may begin to slow the Umbraco backoffice, and you may want to consider {0} or manually removing older content versions from the database.", IsUnversionInstalled ? "[Updating your 'Our.Umbraco.Unversion' configuration" : "Installing 'Our.Umbraco.Unversion'");
            }

            var actions = new List<HealthCheckAction>();

            return
               new HealthCheckStatus(message)
               {
                   ResultType = resultType,
                   Actions = actions
               };
        }

        private HealthCheckStatus CheckMostVersionedContent(bool isUnversionInstalled, int noOfVersions)
        {
            StatusResultType resultType = StatusResultType.Info;
            int noOfContentItemsWithMoreThan = 0;
            VersionInfo highestVersionedContentItem = default(VersionInfo);
            int highestNumberOfVersions = 1;
            IEnumerable<VersionInfo> contentWithMoreVersionsThan = GetContentWithMoreVersionsThan(noOfVersions);
            var message = "You have {0} content items with more than {1} versions - The highest versioned content item is '{2}' with {3} versions";
            if (contentWithMoreVersionsThan != null && contentWithMoreVersionsThan.Any())
            {
                resultType = StatusResultType.Warning;
                noOfContentItemsWithMoreThan = contentWithMoreVersionsThan.Count();
                highestVersionedContentItem = contentWithMoreVersionsThan.FirstOrDefault();
                highestNumberOfVersions = highestVersionedContentItem != null ? highestVersionedContentItem.VersionCount : 1;
                message = String.Format(message, noOfContentItemsWithMoreThan, noOfVersions, highestVersionedContentItem.text, highestNumberOfVersions);
            }
            else
            {
                resultType = StatusResultType.Info;
                message = String.Format("You don't have any 'overly versioned' individual content items, each item has less than {0} content versions.", noOfVersions);

            }
            var actions = new List<HealthCheckAction>();

            return
               new HealthCheckStatus(message)
               {
                   ResultType = resultType,
                   Actions = actions
               };
        }
        #endregion

        /// WIP what actions can we offer to correct versions
        /// was thinking maybe a downloadable report
        /// or link to unversion if it isn't installed
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "viewContentVersionsReport":
                    return GenerateContentVersionReport();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public HealthCheckStatus GenerateContentVersionReport()
        {
            /// WIP what actions can we offer to correct versions
            /// was thinking maybe a downloadable report
            /// or link to unversion if it isn't installed
            return new HealthCheckStatus("What would be useful?");
        }
        public class VersionInfo
        {
            public string ContentId { get; set; }
            public string text { get; set; }
            public int VersionCount { get; set; }


        }
    }
}

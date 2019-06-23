using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.PublishedCache;

namespace Our.Umbraco.HealthChecks.Checks.DataIntegrity
{
    [HealthCheck(
        "d6af141b-d330-4db1-a35f-5cbefd85d04a",
        "Media Integrity Check",
        Description = "Check for any orphaned Media on disk",
        Group = "Media")]
    public class MediaIntegrityCheck : HealthCheck
    {
        private readonly UmbracoDatabase _db;
        private readonly ILocalizedTextService _textService;
        //Keeping Umbraco Version for now might come in handy if I decide the query the database
        private readonly Version _umbracoVersion;
        private readonly IMediaService _mediaService;
        private readonly ContextualPublishedCache _umbracoCache;
        private readonly string _webRoot;
        public MediaIntegrityCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
            _db = healthCheckContext.ApplicationContext.DatabaseContext.Database;
            _umbracoVersion = UmbracoVersion.Current;
            _mediaService = healthCheckContext.ApplicationContext.Services.MediaService;
            _umbracoCache = healthCheckContext.UmbracoContext.ContentCache;
            _webRoot = IOHelper.MapPath("/");
        }
        //TODO: Organise regions better
        #region Discovery

        #region Disk Checking
        /// <summary>
        /// Obtains the location of the Media folder and stores all Files inside a HashSet
        /// </summary>
        /// <returns>HashSet of all items discovered in the media folder</returns>
        private HashSet<string> ScanMediaOnDisk()
        {
            //TODO: Get name of media folder as defined in the FileSystemProviders.config in case a site has a different name for their media
            //TODO: Test with site using a Virtual Directory in IIS (This will probably fail.)
            // Will fail if the media folder is lowercase
            DirectoryInfo directoryInfo = new DirectoryInfo(IOHelper.MapPath("/Media"));

            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            DirectoryInfo[] mediaFolders = directoryInfo.GetDirectories();

            if (mediaFolders.Length == 0)
            {
                throw new FileNotFoundException();
            }

            var mediaFiles = new HashSet<string>();

            foreach (string file in Directory.EnumerateFiles(directoryInfo.FullName, "*.*", SearchOption.AllDirectories))
            {
                //Prevent any .config files from being included in the disk results
                if (!Regex.IsMatch(file, @".[c|C]onfig"))
                {
                    StringBuilder cleanedFile = new StringBuilder(file);
                    //Remove web root path from media path
                    cleanedFile.Replace(_webRoot, "");
                    //Change backslash to forward slash in path
                    cleanedFile.Replace("\\", "/");
                    cleanedFile.Replace("Media", "media");

                    mediaFiles.Add(cleanedFile.ToString());
                }

            }

            return mediaFiles;
        }

        #endregion

        #endregion

        #region Healthchecks
        /// <summary>
        /// Takes a hashset of media item paths from disk and queries these paths against the media service and umbraco.confg
        /// TODO: Figure out a way to accruately query the Examine Index without using a query that starts with a wildcard (Examine would be a better alternative to the XML cache)
        /// </summary>
        /// <returns>HealthCheck Status for this check</returns>
        private HealthCheckStatus CheckMediaIntegrity()
        {
            string resultMessage = string.Empty;
            int found = 0;
            HashSet<string> mediaOnDisk = ScanMediaOnDisk();
            foreach (var item in mediaOnDisk)
            {
                //Query the media service first
                var media = _mediaService.GetMediaByPath(item);

                if (media == null)
                {
                    //Query the umbraco.config to see if there is any references for the specific media item within the content
                    //TODO: Need to check for images inside siteLogo element of cache
                    var mediaInCache = _umbracoCache.GetByXPath("//content[text()[contains(.,'"+ item + "')]]/parent::*");
                    if (mediaInCache.Count() != 0)
                    {
                        found += 1;
                    }
                }
                else
                {
                    found += 1;
                }

            }

            StatusResultType resultType = StatusResultType.Warning;
            if (mediaOnDisk.Count() == found)
            {
                resultType = StatusResultType.Info;
            }
            var actions = new List<HealthCheckAction>();
            resultMessage = String.Format("Found {0} media items on disk and {1} items are currently being used by Umbraco", mediaOnDisk.Count().ToString(), found);
            return
                new HealthCheckStatus(resultMessage)
                {
                    ResultType = resultType,
                    Actions = actions
                };
        }

        #endregion

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckMediaIntegrity() };
        }
    }
}

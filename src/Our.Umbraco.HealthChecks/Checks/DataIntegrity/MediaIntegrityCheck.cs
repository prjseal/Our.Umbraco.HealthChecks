using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Examine.Providers;
using Newtonsoft.Json.Linq;
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
        Description = "Check for any orphaned Media on disk, please ensure you have performed a full site republish before running this test, you can do this by visiting: /umbraco/dialogs/republish.aspx?xml=true and clicking republish entire site",
        Group = "Media")]
    public class MediaIntegrityCheck : HealthCheck
    {
        private readonly UmbracoDatabase _db;
        private readonly ILocalizedTextService _textService;
        private const string MoveOrphanedMediaAction = "moveOrphanedMedia";
        //Keeping Umbraco Version for now might come in handy if I decide the query the database
        private readonly Version _umbracoVersion;
        private readonly IMediaService _mediaService;
        private readonly ContextualPublishedCache _umbracoCache;
        private readonly string _webRoot;
        private readonly BaseSearchProvider _internalIndex;
        public MediaIntegrityCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
            _db = healthCheckContext.ApplicationContext.DatabaseContext.Database;
            _umbracoVersion = UmbracoVersion.Current;
            _mediaService = healthCheckContext.ApplicationContext.Services.MediaService;
            _umbracoCache = healthCheckContext.UmbracoContext.ContentCache;
            _webRoot = IOHelper.MapPath("/");
            _internalIndex = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"];
        }
        //TODO: Organize regions better
        #region Discovery
            /// <summary>
            /// Will query the Examine InternalIndex for media items
            /// A media item has the key __IndexType with the value of media
            /// So we query for items in the index using this key and were able to pull all media from examine
            /// We then apply some regex to grab just the path to the media item and store it in a hashset
            /// </summary>
            /// <returns>
            /// A hashset containing paths to media or an empty hashset if nothing is found
            /// </returns>
        private HashSet<string> QueryMediaFromInternalIndex()
        {
            HashSet<string> mediaItems = new HashSet<string>();
            var searchCriteria = _internalIndex.CreateSearchCriteria();
            var query = searchCriteria.RawQuery("+__IndexType:media");
            var searchResults = _internalIndex.Search(query);
            if (searchResults.Any())
            {
                foreach (var itemResult in searchResults)
                {
                    // \/[M|m]edia\/[0-9]+\/([^']+)
                    // This regex pattern will match the media path within the umbracoFile entry in the examine search {src: '/media/1050/myimage.jpg', crops: []}
                    // In the above example /media/1050/myimage.jpg will be extracted from the entry
                    mediaItems.Add(Regex.Match(itemResult.Fields["umbracoFile"], @"[M|m]edia\/[0-9]+\/([^']+)").Value);
                }

                return mediaItems;
            }
            return new HashSet<string>();
        }

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
                StringBuilder filPath = new StringBuilder(file);
                //Remove web root path from media path
                filPath.Replace(_webRoot, "");
                //Change backslash to forward slash in path
                filPath.Replace("\\", "/");
                filPath.Replace("Media", "media");
                string cleanedFilePath = filPath.ToString();
                //Path must start with Media or media followed by a / then any length of number then a / followed by any number of characters
                if (Regex.IsMatch(cleanedFilePath, @"^[M|m]edia\/[0-9]+\/.+"))
                {
                    mediaFiles.Add(cleanedFilePath);
                }

            }

            return mediaFiles;
        }

        #endregion

        #endregion

        #region Healthchecks
        /// <summary>
        /// Takes a hashset of media item paths from disk and queries these paths against the media service and umbraco.confg
        /// TODO: Figure out a way to accurately query the Examine Internal Index without using a query that starts with a wildcard (Examine would be a better alternative to the XML cache)
        /// </summary>
        /// <returns>HealthCheck Status for this check</returns>
        private HealthCheckStatus CheckMediaIntegrity()
        {
            bool success = false;
            string resultMessage = string.Empty;
            int foundInContent = 0;
            int foundInSiteLogoContent = 0;
            int foundInMedia = 0;
            HashSet<string> mediaOnDisk = ScanMediaOnDisk();
            HashSet<string> mediaInIndex = QueryMediaFromInternalIndex();
            HashSet<string> orphanedMediaItems = new HashSet<string>();
            foreach (var item in mediaOnDisk)
            {

                if (!mediaInIndex.Contains(item))
                {
                    //Query the umbraco.config to see if there is any references for the specific media item within the content
                    //TODO: Need to check for images inside siteLogo element of cache
                    //Not found in Internal Index
                    var mediaInCache = _umbracoCache.GetByXPath("//content[text()[contains(.,'" + item + "')]]/parent::*");
                    if (mediaInCache.Count() != 0)
                    {
                        //Found in umbraco.config
                            foundInContent += 1;
                    }
                    else
                    {
                        mediaInCache = _umbracoCache.GetByXPath("//siteLogo[text()[contains(.,'" + item + "')]]/parent::*");
                        if (mediaInCache.Count() != 0)
                        {
                            foundInSiteLogoContent += 1;
                        }
                        else
                        {
                            //Not found in Content
                            //Query CMSMedia table as a last resort
                            orphanedMediaItems.Add(item);
                        }
                        

                    }
                }
                else
                {
                    //Found in Internal Index
                    foundInMedia += 1;
                }

            }
            success = orphanedMediaItems.Count == 0;
            //StatusResultType resultType = StatusResultType.Warning;
            var actions = new List<HealthCheckAction>();
            var parameters = new Dictionary<string, object>();
            parameters.Add("flaggedMediaItems", orphanedMediaItems);
            if (success == false)
            {
                actions.Add(new HealthCheckAction(MoveOrphanedMediaAction, Id)
                {
                    Name = _textService.Localize("Our.Umbraco.HealthChecks/moveOrphanedMedia"),
                    Description = _textService.Localize("Our.Umbraco.HealthChecks/orphanedMediaDescription"),
                    ActionParameters = parameters,
                });
            }
            resultMessage = String.Format("Found {0} media items on disk. I found {1} items within content, {2} in the site logo content and I found {3} items in the internal index, {4} items appear to be orphaned", mediaOnDisk.Count().ToString(), foundInContent, foundInSiteLogoContent, foundInMedia, orphanedMediaItems.Count);
            return
                new HealthCheckStatus(resultMessage)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        #endregion

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case MoveOrphanedMediaAction:
                    JArray flaggedMedia = (JArray)action.ActionParameters["flaggedMediaItems"];
                    return MoveOrphanedMedia(flaggedMedia);
                default:
                    throw new InvalidOperationException("Move Orphaned Media action requested is either not executable or does not exist");
            }
        }

        private HealthCheckStatus MoveOrphanedMedia(JArray flaggedMediaItems)
        {
            //HashSet<string> flaggedMediaItems1 = flaggedMediaItems as HashSet<string>;
            DirectoryInfo mediaFolder = new DirectoryInfo(IOHelper.MapPath("/Media"));

            if (!mediaFolder.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            //Attempt to create a folder outside the web root
            DirectoryInfo webRoot = new DirectoryInfo(_webRoot);
            string targetMediaFolderName = "badMedia-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            DirectoryInfo mediaDeposit = new DirectoryInfo(webRoot.Parent.FullName + "\\media-cleaner\\"+ targetMediaFolderName);
            mediaDeposit.Create();
            foreach (string item in flaggedMediaItems)
            {
                string source = IOHelper.MapPath("~/"+item);
                string destination = mediaDeposit.FullName + "\\" + Regex.Replace(item,@"/",@"\");

                DirectoryInfo file = new DirectoryInfo(Path.GetDirectoryName(destination));
                file.Create();

                File.Move(source, destination);
            }

            return
                    new HealthCheckStatus(_textService.Localize("Our.Umbraco.HealthChecks/moveOrphanedMediaSuccess"))
                    {
                        ResultType = StatusResultType.Success
                    };

            /*
             * TODO: Log error condition here 
             */
            //return
            //        new HealthCheckStatus(_textService.Localize("Our.Umbraco.HealthChecks/moveOrphanedMediaError"))
            //        {
            //            ResultType = StatusResultType.Error
            //        };
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckMediaIntegrity() };
        }
    }
}

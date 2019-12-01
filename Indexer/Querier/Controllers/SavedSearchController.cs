using System.Web.Http;
using System.Collections.Generic;
using wanshitong;
using System.Linq;
using Indexer.LuceneTools;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.DataContracts;
using System.Threading.Tasks;

namespace Indexer.Querier.Controllers
{
    public class SavedSearchController : ApiController
    {
        [HttpGet]
        public bool AddSavedSearch(string phrase)
        {
            Telemetry.Instance.TrackEvent("SavedSearchController.AddSavedSearch");
            
            try
            {
                List<SavedSearchModel> savedSearches;
                // for now just do simple storage. 
                if (Storage.Instance.Exists("savedSearches"))
                {
                    savedSearches = Storage.Instance.Get<List<SavedSearchModel>>("savedSearches");
                }
                else
                {
                    savedSearches = new List<SavedSearchModel>();
                }

                savedSearches.Add(new SavedSearchModel{ SearchPhrase = phrase });
                savedSearches = savedSearches.ToHashSet().ToList();

                Storage.Instance.Store("savedSearches", savedSearches);
                Storage.Instance.Persist();
            }
            catch (System.Exception e)
            {
                Telemetry.Instance.TrackTrace("SavedSearchController.AddSavedSearch.Error", SeverityLevel.Error);
                Telemetry.Instance.TrackException(e);

                return false;
            }

            return true;
        }

        [HttpGet]
        public List<SavedSearchModel> GetSavedSearches()
        {
            Telemetry.Instance.TrackEvent("SavedSearchController.GetSavedSearches");

            List<SavedSearchModel> savedSearches;
            try
            {
                // for now just do simple storage. 
                if (Storage.Instance.Exists("savedSearches"))
                {
                    savedSearches = Storage.Instance.Get<List<SavedSearchModel>>("savedSearches");
                }
                else
                {
                    savedSearches = new List<SavedSearchModel>();
                }
            }
            catch (System.Exception e)
            {
                Telemetry.Instance.TrackTrace("SavedSearchController.AddSavedSearch.Error", SeverityLevel.Error);
                Telemetry.Instance.TrackException(e);

                return null;
            }

            return savedSearches;
        }
    }
}
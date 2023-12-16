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
        private readonly Storage storage;
        private readonly Telemetry telemetry;


        public SavedSearchController(Storage storage, Telemetry telemetry)
        {
            this.storage = storage;
            this.telemetry = telemetry;

        }

        [HttpGet]
        public bool AddSavedSearch(string phrase)
        {
            this.telemetry.client.TrackEvent("SavedSearchController.AddSavedSearch");
            
            try
            {
                List<SavedSearchModel> savedSearches;
                // for now just do simple storage. 
                if (this.storage.Instance.Exists("savedSearches"))
                {
                    savedSearches = this.storage.Instance.Get<List<SavedSearchModel>>("savedSearches");
                }
                else
                {
                    savedSearches = new List<SavedSearchModel>();
                }

                savedSearches.Add(new SavedSearchModel{ SearchPhrase = phrase });
                savedSearches = savedSearches.ToHashSet().ToList();

                this.storage.Instance.Store("savedSearches", savedSearches);
                this.storage.Instance.Persist();
            }
            catch (System.Exception e)
            {
                this.telemetry.client.TrackTrace("SavedSearchController.AddSavedSearch.Error", SeverityLevel.Error);
                this.telemetry.client.TrackException(e);

                return false;
            }

            return true;
        }

        [HttpGet]
        public List<SavedSearchModel> GetSavedSearches()
        {
            this.telemetry.client.TrackEvent("SavedSearchController.GetSavedSearches");

            List<SavedSearchModel> savedSearches;
            try
            {
                // for now just do simple storage. 
                if (this.storage.Instance.Exists("savedSearches"))
                {
                    savedSearches = this.storage.Instance.Get<List<SavedSearchModel>>("savedSearches");
                }
                else
                {
                    savedSearches = new List<SavedSearchModel>();
                }
            }
            catch (System.Exception e)
            {
                this.telemetry.client.TrackTrace("SavedSearchController.AddSavedSearch.Error", SeverityLevel.Error);
                this.telemetry.client.TrackException(e);

                return null;
            }

            return savedSearches;
        }
    }
}
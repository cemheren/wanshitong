using System.Web.Http;
using System.Collections.Generic;
using wanshitong;
using System.Linq;
using Indexer.LuceneTools;
using System;
using Microsoft.AspNetCore.Mvc;
using wanshitong.Common.Lucene;

namespace Indexer.Querier.Controllers
{
    public class QueryController : ApiController
    {
        private readonly Telemetry telemetry;
        private readonly LuceneClient luceneClient;

        public QueryController(Telemetry telemetry, LuceneClient luceneClient)
        {
            this.telemetry = telemetry;
            this.luceneClient = luceneClient;
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public List<SearchModel> SearchText([Microsoft.AspNetCore.Mvc.FromBody]SavedSearchModel searchModel)
        {
            this.telemetry.client.TrackEvent("QueryController.SearchText");

            var searchResults = this.luceneClient.Search(searchModel.SearchPhrase);

            // Hacky way to group results, eventually can migrate to lucene.
            if (searchModel.GroupingPhrase != null)
            {
                foreach (var searchResult in searchResults)
                {
                    searchResult.GroupingNumber = searchResult.Tags.Contains(searchModel.GroupingPhrase, StringComparer.OrdinalIgnoreCase) ? 0 : 1;
                }
            }

            return searchResults;            
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public List<SearchModel> SearchWithDates(DateTime start, DateTime end)
        {
            this.telemetry.client.TrackEvent("QueryController.SearchWithDates");

            var searchResults = this.luceneClient.SearchWithIngestionTime(start, end);

            return searchResults;            
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public List<SearchModel> GetAll()
        {
            this.telemetry.client.TrackEvent("QueryController.GetAll");

            var searchResults = this.luceneClient.GetAll();

            return searchResults;  
        }

        [Microsoft.AspNetCore.Mvc.HttpDelete]
        public void Delete(int docId)
        {
            this.telemetry.client.TrackEvent("QueryController.Delete");
            
            this.luceneClient.Delete(docId);
        }

        [Microsoft.AspNetCore.Mvc.ActionName("TagDocs")]
        public ActionResult<bool> TagDocs([Microsoft.AspNetCore.Mvc.FromBody]TagDocModel tagDocModel)
        {
            this.telemetry.client.TrackEvent("QueryController.TagDocs");

            System.Console.WriteLine(tagDocModel);
            foreach (var myId in tagDocModel.IndexAndDocId.Values)
            {
                var currentDocument = this.luceneClient.SearchWithMyId(myId);

                var hs = currentDocument.Tags.ToHashSet();
                hs.Add(tagDocModel.Tag);

                currentDocument.Tags = hs.ToArray();
                currentDocument.Metadata.TagOrders = tagDocModel.IndexAndDocId;

                this.luceneClient.UpdateDocument(currentDocument, true);
            }

            return true;
        }
    } 
}
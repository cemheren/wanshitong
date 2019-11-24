using System.Web.Http;
using System.Collections.Generic;
using wanshitong;
using System.Linq;
using Indexer.LuceneTools;
using System;
using Microsoft.AspNetCore.Mvc;

namespace Indexer.Querier.Controllers
{
    public class QueryController : ApiController
    {
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public List<SearchModel> SearchText(string text)
        {
            Telemetry.Instance.TrackEvent("QueryController.SearchText");

            var searchResults = Program
                .m_luceneTools
                .Search(text);

            return searchResults;            
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public List<SearchModel> SearchWithDates(DateTime start, DateTime end)
        {
            Telemetry.Instance.TrackEvent("QueryController.SearchWithDates");

            var searchResults = Program
                .m_luceneTools
                .SearchWithIngestionTime(start, end);

            return searchResults;            
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public List<SearchModel> GetAll()
        {
            Telemetry.Instance.TrackEvent("QueryController.GetAll");

            var searchResults = Program
                .m_luceneTools
                .GetAll();

            return searchResults;  
        }

        [Microsoft.AspNetCore.Mvc.HttpDelete]
        public void Delete(int docId)
        {
            Telemetry.Instance.TrackEvent("QueryController.Delete");
            
            Program
                .m_luceneTools
                .Delete(docId);
        }

        [Microsoft.AspNetCore.Mvc.ActionName("TagDocs")]
        public ActionResult<bool> TagDocs([Microsoft.AspNetCore.Mvc.FromBody]TagDocModel tagDocModel)
        {
            Telemetry.Instance.TrackEvent("QueryController.TagDocs");

            System.Console.WriteLine(tagDocModel);
            foreach (var myId in tagDocModel.IndexAndDocId.Values)
            {
                var currentDocument = Program
                    .m_luceneTools
                    .SearchWithMyId(myId);

                var hs = currentDocument.Tags.ToHashSet();
                hs.Add(tagDocModel.Tag);

                currentDocument.Tags = hs.ToArray();
                currentDocument.Metadata.TagOrders = tagDocModel.IndexAndDocId;

                Program
                    .m_luceneTools
                    .UpdateDocument(currentDocument);
            }

            return true;
        }
    } 
}
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
            var searchResults = Program
                .m_luceneTools
                .Search(text);

            return searchResults;            
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public List<SearchModel> SearchWithDates(DateTime start, DateTime end)
        {
            var searchResults = Program
                .m_luceneTools
                .SearchWithIngestionTime(start, end);

            return searchResults;            
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public List<SearchModel> GetAll()
        {
            var searchResults = Program
                .m_luceneTools
                .GetAll();

            return searchResults;  
        }

        [Microsoft.AspNetCore.Mvc.HttpDelete]
        public void Delete(int docId)
        {
             Program
                .m_luceneTools
                .Delete(docId);
        }

        [Microsoft.AspNetCore.Mvc.ActionName("TagDocs")]
        public ActionResult<bool> TagDocs([Microsoft.AspNetCore.Mvc.FromBody]TagDocModel tagDocModel)
        {
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
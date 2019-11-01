using System.Web.Http;
using System.Collections.Generic;
using wanshitong;
using System.Linq;
using Indexer.LuceneTools;
using System;

namespace Indexer.Querier.Controllers
{
    public class QueryController : ApiController
    {
        [HttpGet]
        public List<SearchModel> SearchText(string text)
        {
            var searchResults = Program
                .m_luceneTools
                .Search(text);

            return searchResults;            
        }

        [HttpGet]
        public List<SearchModel> SearchWithDates(DateTime start, DateTime end)
        {
            var searchResults = Program
                .m_luceneTools
                .SearchWithIngestionTime(start, end);

            return searchResults;            
        }

        [HttpGet]
        public List<SearchModel> GetAll()
        {
            var searchResults = Program
                .m_luceneTools
                .GetAll();

            return searchResults;  
        }

        [HttpDelete]
        public void Delete(int docId)
        {
             Program
                .m_luceneTools
                .Delete(docId);
        }

        [HttpPost]
        public void TagDocs([FromBody]TagDocModel tagDocModel)
        {
            foreach (var myId in tagDocModel.indexAndDocId.Values)
            {
                var currentDocument = Program
                    .m_luceneTools
                    .SearchWithMyId(myId);

                var hs = currentDocument.Tags.ToHashSet();
                hs.Add(tagDocModel.tag);

                currentDocument.Tags = hs.ToArray();
                currentDocument.Metadata.TagOrders = tagDocModel.indexAndDocId;

                Program
                    .m_luceneTools
                    .UpdateDocument(currentDocument);
            }
        }
    } 
}
using System.Web.Http;
using System.Collections.Generic;
using wanshitong;
using System.Linq;

namespace Indexer.Querier.Controllers
{
    public class SearchModal
    {
        public static SearchModal FromTuple((string, string) tuple)
        {
            return new SearchModal
            {
                Group = tuple.Item1,
                Text = tuple.Item2
            };
        }

        public string Group { get; set; }

        public string Text { get; set; }
    }

    public class QueryController : ApiController
    {
        [HttpGet]
        public List<SearchModal> SearchText(string text)
        {
            var searchResults = Program
                .m_luceneTools
                .Search(text);

            return searchResults.Select(r => SearchModal.FromTuple(r)).ToList();            
        }

        [HttpGet]
        public List<SearchModal> GetAll()
        {
            var searchResults = Program
                .m_luceneTools
                .GetAll();

            return searchResults.Select(r => SearchModal.FromTuple(r)).ToList();  
        }
    } 
}
using System;
using Lucene.Net.Documents;

namespace Indexer.LuceneTools
{
    public class SearchModel
    {
        public static SearchModel FromDoc(Document doc, int id)
        {
            return new SearchModel
            {
                DocId = id,
                Group = doc.Get("group"),
                Text = doc.Get("text"),
                ProcessId = doc.Get("processId"),
                IngestionTime = DateTools.StringToDate(doc.Get("ingestionTime")), 
            };
        }

        public string Group { get; set; }

        public string Text { get; set; }
        
        public int DocId { get; set; }

        public string ProcessId { get; set; }

        public DateTime IngestionTime { get; set; }
    }
}
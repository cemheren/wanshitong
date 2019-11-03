using System;
using System.Collections.Generic;
using Lucene.Net.Documents;
using Newtonsoft.Json;

namespace Indexer.LuceneTools
{
    public class Metadata
    {
        // (order, docid)
        public Dictionary<string, string> TagOrders { get; set; }
    }

    public class SearchModel
    {
        public static SearchModel FromDoc(Document doc, int id)
        {
            var tags = doc.GetValues("tags");
            Metadata jsonMetadata = null;
            if(doc.Get("metadata") != null)
            {
                jsonMetadata = JsonConvert.DeserializeObject<Metadata>(doc.Get("metadata"));
            }else
            {
                jsonMetadata = new Metadata();
            }

            return new SearchModel
            {
                DocId = id,
                MyId = doc.Get("ID"),
                Group = doc.Get("group"),
                Text = doc.Get("text"),
                ProcessId = doc.Get("processId"),
                IngestionTime = DateTools.StringToDate(doc.Get("ingestionTime")), 
                Tags = tags,
                Metadata = jsonMetadata
            };
        }

        public string Group { get; set; }

        public string Text { get; set; }
        
        public string[] Tags { get; set; }

        public int DocId { get; set; }

        public string MyId { get; set; }

        public string ProcessId { get; set; }

        public DateTime IngestionTime { get; set; }


// Non Indexed fields
        public string HighlightedText { get; set; }
        
        public Metadata Metadata { get; set; }
    }
}
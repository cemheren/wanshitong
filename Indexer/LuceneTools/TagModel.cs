using System;
using System.Collections.Generic;
using Lucene.Net.Documents;
using Newtonsoft.Json;

namespace Indexer.LuceneTools
{
    public class TagModel
    {
        public static TagModel FromDoc(Document doc, int id)
        {
            return new TagModel
            {
                DocId = id,
                Tag = doc.Get("tag"),
                Color = doc.Get("color"),
                Type = "Tag"
            };
        }

        public static Document ToDoc(TagModel model)
        {
            var doc = new Document();

            doc.Add(new StringField("type", model.Type, Field.Store.YES));
            doc.Add(new StringField("tag", model.Tag, Field.Store.YES));
            doc.Add(new StringField("color", model.Color, Field.Store.YES));

            return doc;
        }

        public int DocId { get; set; }

        public string Type { get; set; }

        public string Tag { get; set; }

        public string Color { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Indexer.LuceneTools;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Util;
using Lucene.Net.Facet;
using Newtonsoft.Json;

namespace wanshitong.Common.Lucene
{
    public partial class LuceneClient
    {
        public void AddTag(string tag, string color)
        {
            using (var writer = new IndexWriter(dir, new IndexWriterConfig(appLuceneVersion, analyzer)))
            {
                var doc = new Document();

                doc.Add(new StringField("type", "Tag", Field.Store.YES));
                doc.Add(new StringField("tag", tag, Field.Store.YES));
                doc.Add(new StringField("color", color, Field.Store.YES));

                writer.AddDocument(doc);
                writer.Commit();
                writer.Flush(triggerMerge: false, applyAllDeletes: false);
            }
        }
        
        public List<TagModel> SearchTag(string s)
        {
            var results = new List<TagModel>();
            var wildcard = s.Contains("*") || s.Contains("?");
            
            Query phrase;
            if(wildcard)
            {
                phrase = new WildcardQuery(new Term("tag", s));
            }else
            {
                phrase= new BooleanQuery() {
                    new BooleanClause(new TermQuery(new Term("tag", s)), Occur.SHOULD)
                };
            }

            using (var reader = DirectoryReader.Open(this.dir))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(phrase, 12).ScoreDocs;
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    var tagModel = TagModel.FromDoc(foundDoc, hit.Doc);

                    results.Add(tagModel);
                }
            }

            return results;
        }
    }
}
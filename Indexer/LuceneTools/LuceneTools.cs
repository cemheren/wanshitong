using System.Collections.Generic;
using System.IO;
using Indexer.LuceneTools;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace wanshitong.Common.Lucene
{
    public class LuceneTools
    {
        private FSDirectory dir;
        private StandardAnalyzer analyzer;
        private LuceneVersion appLuceneVersion;

        public LuceneTools()
        {
        }

        public void InitializeIndex()
        {
            this.appLuceneVersion = LuceneVersion.LUCENE_48;
            
            var currentDir = "/Users/cemheren";

            var dirInfo = new DirectoryInfo(Path.Combine(currentDir, "Index"));

            System.Console.WriteLine($"Using index folder {dirInfo.FullName}");

            this.dir = FSDirectory.Open(dirInfo);

            //create an analyzer to process the text
            this.analyzer = new StandardAnalyzer(appLuceneVersion);
        }

        public void AddAndCommit(string group, string text)
        {
            using (var writer = new IndexWriter(dir, new IndexWriterConfig(appLuceneVersion, analyzer)))
            {
                var doc = new Document();

                // StringField indexes but doesn't tokenise
                doc.Add(new StringField("group", group, Field.Store.YES));
                doc.Add(new TextField("text", text, Field.Store.YES));

                writer.AddDocument(doc);
                writer.Commit();
                writer.Flush(triggerMerge: false, applyAllDeletes: false);
            }
        }

        public void Delete(int docID)
        {
            using (var writer = new IndexWriter(dir, new IndexWriterConfig(appLuceneVersion, analyzer)))
            {
                var isDeleted = writer.TryDeleteDocument(writer.GetReader(false), docID);
                writer.Commit();
                writer.Flush(triggerMerge: false, applyAllDeletes: true);
            }
        }

        public List<SearchModel> Search(string s)
        {
            var results = new List<SearchModel>();

            Query phrase;
            if(s.Contains("*") || s.Contains("?"))
            {
                phrase = new WildcardQuery(new Term("text", s));
            }else //if(s.Length > 1)
            {
                var parser = new QueryParser(LuceneVersion.LUCENE_48, "text", new StandardAnalyzer(LuceneVersion.LUCENE_48));
                try
                {
                    phrase = parser.Parse(s);                    
                }
                catch (System.Exception)
                {
                    return results;
                } 
            }

            using (var reader = DirectoryReader.Open(this.dir))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(phrase, 50).ScoreDocs;
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    System.Console.WriteLine($"{foundDoc.Get("group")}: {foundDoc.Get("text")}");
                    results.Add(
                        new SearchModel
                        {
                            Group = foundDoc.Get("group"),
                            Text = foundDoc.Get("text"),
                            DocId = hit.Doc
                        });
                }
            }

            return results;
        }

        public List<SearchModel> GetAll()
        {
            var query = new MatchAllDocsQuery();
            var results = new List<SearchModel>();

            using (var reader = DirectoryReader.Open(this.dir))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(query, 1000).ScoreDocs;
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    System.Console.WriteLine($"{foundDoc.Get("group")}: {foundDoc.Get("text")}");
                    results.Add(
                        new SearchModel
                        {
                            Group = foundDoc.Get("group"),
                            Text = foundDoc.Get("text")
                        });
                }
            }

            return results;
        }

        public void SearchGroup(params string[] s)
        {
            var phrase = new MultiPhraseQuery();
            phrase.Add(new Term("group", s[0]));

            using (var reader = DirectoryReader.Open(this.dir))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(phrase, 20 /* top 20 */).ScoreDocs;
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    System.Console.WriteLine($"{foundDoc.Get("group")}: {foundDoc.Get("text")}");
                }
            }
        }
    }
}
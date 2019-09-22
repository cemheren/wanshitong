using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
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

        public List<(string group, string text)> Search(params string[] s)
        {
            var phrase = new MultiPhraseQuery();
            phrase.Add(new Term("text", s[0]));
            var results = new List<(string group, string text)>();

            using (var reader = DirectoryReader.Open(this.dir))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(phrase, 20).ScoreDocs;
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    System.Console.WriteLine($"{foundDoc.Get("group")}: {foundDoc.Get("text")}");
                    results.Add((group: foundDoc.Get("group"), text: foundDoc.Get("text")));
                }
            }

            return results;
        }

        public List<(string group, string text)> GetAll()
        {
            var query = new MatchAllDocsQuery();
            var results = new List<(string group, string text)>();

            using (var reader = DirectoryReader.Open(this.dir))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(query, 1000).ScoreDocs;
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    System.Console.WriteLine($"{foundDoc.Get("group")}: {foundDoc.Get("text")}");
                    results.Add((group: foundDoc.Get("group"), text: foundDoc.Get("text")));
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
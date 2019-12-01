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
    public partial class LuceneTools
    {
        private FSDirectory dir;
        private CodeAwareAnalyzer analyzer;
        private LuceneVersion appLuceneVersion;

        public LuceneTools()
        {
        }

        public void InitializeIndex()
        {
            this.appLuceneVersion = LuceneVersion.LUCENE_48;
            
            var currentDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var dirInfo = new DirectoryInfo(Path.Combine(currentDir, "Index"));

            System.Console.WriteLine($"Using index folder {dirInfo.FullName}");

            this.dir = FSDirectory.Open(dirInfo);

            //create an analyzer to process the text
            this.analyzer = new CodeAwareAnalyzer(appLuceneVersion);
        }

        public void AddAndCommit(string group, string text, int processId)
        {
            using (var writer = new IndexWriter(dir, new IndexWriterConfig(appLuceneVersion, analyzer)))
            {
                var doc = new Document();

                doc.Add(new StringField("type", "Document", Field.Store.YES));
                doc.Add(new StringField("ID", Guid.NewGuid().ToString(), Field.Store.YES));
                doc.Add(new StringField("group", group, Field.Store.YES));
                doc.Add(new StringField("processId", processId.ToString(), Field.Store.YES));
                doc.Add(new TextField("text", text, Field.Store.YES));

                // add a non-searchable metadata field

                doc.Add(new StringField("ingestionTime",
                     DateTools.DateToString(DateTime.UtcNow, DateTools.Resolution.SECOND), Field.Store.YES));

                writer.AddDocument(doc);
                writer.Commit();
                writer.Flush(triggerMerge: false, applyAllDeletes: false);
            }
        }

        public void MigrateDocuments(List<SearchModel> migratedDocuments)
        {
            using (var writer = new IndexWriter(dir, new IndexWriterConfig(appLuceneVersion, analyzer)))
            {
                foreach (var migratedDocument in migratedDocuments)
                {
                    if (migratedDocument.MyId == null  || migratedDocument.Type == null)
                    {
                        var doc = new Document();
                        var isDeleted = writer.TryDeleteDocument(writer.GetReader(false), migratedDocument.DocId);
                        
                        migratedDocument.MyId = migratedDocument.MyId ?? Guid.NewGuid().ToString();
                        migratedDocument.Type = "Document";

                        doc.Add(new StringField("ID", migratedDocument.MyId, Field.Store.YES));
                        doc.Add(new StringField("type", migratedDocument.Type, Field.Store.YES));
                        doc.Add(new StringField("group", migratedDocument.Group, Field.Store.YES));
                        doc.Add(new StringField("processId", migratedDocument.ProcessId, Field.Store.YES));
                        doc.Add(new TextField("text", migratedDocument.Text, Field.Store.YES));

                        doc.Add(new StringField("metadata", JsonConvert.SerializeObject(migratedDocument.Metadata), Field.Store.YES));

                        foreach (var tag in migratedDocument.Tags)
                        {
                            doc.Add(new StringField("tags", tag, Field.Store.YES));
                        }

                        doc.Add(new StringField("ingestionTime",
                            DateTools.DateToString(migratedDocument.IngestionTime, DateTools.Resolution.SECOND), Field.Store.YES));

                        writer.AddDocument(doc);
                    }
                }
                
                writer.Commit();
                writer.Flush(triggerMerge: false, applyAllDeletes: true);
            }
        }

        public string UpdateDocument(SearchModel updatedDocument)
        {
            using (var writer = new IndexWriter(dir, new IndexWriterConfig(appLuceneVersion, analyzer)))
            {
                var doc = new Document();

                var isDeleted = writer.TryDeleteDocument(writer.GetReader(false), updatedDocument.DocId);
                
                updatedDocument.MyId = updatedDocument.MyId ?? Guid.NewGuid().ToString();

                doc.Add(new StringField("type", updatedDocument.Type, Field.Store.YES));
                doc.Add(new StringField("ID", updatedDocument.MyId, Field.Store.YES));
                doc.Add(new StringField("group", updatedDocument.Group, Field.Store.YES));
                doc.Add(new StringField("processId", updatedDocument.ProcessId, Field.Store.YES));
                doc.Add(new TextField("text", updatedDocument.Text, Field.Store.YES));

                doc.Add(new StringField("metadata", JsonConvert.SerializeObject(updatedDocument.Metadata), Field.Store.YES));

                foreach (var tag in updatedDocument.Tags)
                {
                    doc.Add(new TextField("tags", tag, Field.Store.YES));
                }

                doc.Add(new StringField("ingestionTime",
                     DateTools.DateToString(updatedDocument.IngestionTime, DateTools.Resolution.SECOND), Field.Store.YES));

                writer.AddDocument(doc);
                writer.Commit();
                writer.Flush(triggerMerge: false, applyAllDeletes: true);
            
                return updatedDocument.MyId;
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

        public SearchModel SearchWithMyId(String ID)
        {
            var result = default(SearchModel);

            var booleanQuery = new BooleanQuery() {
                new BooleanClause(new TermQuery(new Term("ID", ID.ToString())), Occur.SHOULD)
            };

            using (var reader = DirectoryReader.Open(this.dir))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(booleanQuery, 1).ScoreDocs;
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    var searchModel = SearchModel.FromDoc(foundDoc, hit.Doc);
                        
                    return searchModel;
                }
            }

            return result;
        }

        public List<SearchModel> SearchWithIngestionTime(DateTime start, DateTime end)
        {
            var results = new List<SearchModel>();

            var query = new MatchAllDocsQuery();
            var filter = FieldCacheRangeFilter.NewStringRange("ingestionTime", 
                lowerVal: DateTools.DateToString(start, DateTools.Resolution.SECOND), 
                includeLower: true, 
                upperVal: DateTools.DateToString(end, DateTools.Resolution.SECOND),
                includeUpper: true);

            using (var reader = DirectoryReader.Open(this.dir))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(query, filter, 10).ScoreDocs;
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    var searchModel = SearchModel.FromDoc(foundDoc, hit.Doc);

                    results.Add(SearchModel.FromDoc(foundDoc, hit.Doc));                    
                }
            }

            // migrate
            this.MigrateDocuments(results);

            return results.OrderBy(r => r.IngestionTime).ToList();
        }

        public List<SearchModel> Search(string s)
        {
            var results = new List<SearchModel>();
            var wildcard = s.Trim().Equals("*");
            
            Query phrase;
            if(wildcard)
            {
                phrase = new WildcardQuery(new Term("text", s));
            }
            else 
            {
                var parser = new QueryParser(LuceneVersion.LUCENE_48, "text", this.analyzer);
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
                Storage.Instance.Store("maxdoc", reader.MaxDoc);

                var searcher = new IndexSearcher(reader);
                ScoreDoc[] hits;
                if (wildcard)
                {
                    SortField field = new SortField("ingestionTime", SortFieldType.STRING, true);
                    Sort sort = new Sort(field);

                    hits = searcher.Search(phrase, 50, sort).ScoreDocs;
                }
                else
                {
                    hits = searcher.Search(phrase, 50).ScoreDocs;                
                }

                var formatter = new SimpleHTMLFormatter("<span style=\"background:darkgoldenrod;\">","</span>");
                var fragmenter = new SimpleFragmenter(250);
                var scorer = new QueryScorer(phrase);
                var highlighter = new Highlighter(formatter, scorer);
                highlighter.TextFragmenter = fragmenter;    

                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    var result = SearchModel.FromDoc(foundDoc, hit.Doc);
                    
                    if (!wildcard)
                    {
                         var stream = analyzer.GetTokenStream("", new StringReader(result.Text));
                        string highlightedText = highlighter.GetBestFragments(stream, result.Text, 1, "...");
                        result.HighlightedText = highlightedText;
                    }

                    results.Add(result);
                }
            }

            // migrate
            this.MigrateDocuments(results);

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
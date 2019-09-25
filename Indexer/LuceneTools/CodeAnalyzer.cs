using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Util;

namespace Indexer.LuceneTools
{
    public class CodeAwareAnalyzer : Analyzer
    {
        private readonly LuceneVersion version;

        public CodeAwareAnalyzer(LuceneVersion version)
        {
            this.version = version;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(version, reader);

            TokenStream result = new StandardFilter(version, tokenizer);

            //https://github.com/apache/lucenenet/blob/master/src/Lucene.Net.Analysis.Common/Analysis/Miscellaneous/WordDelimiterFilter.cs
            result = new WordDelimiterFilter(
                version, 
                tokenizer, 
                WordDelimiterFlags.SPLIT_ON_CASE_CHANGE | WordDelimiterFlags.GENERATE_WORD_PARTS, 
                null);

            result = new LowerCaseFilter(version, result);

            return new TokenStreamComponents(tokenizer, result);
        }
    }
}
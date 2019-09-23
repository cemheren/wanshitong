namespace Indexer.LuceneTools
{
    public class SearchModel
    {
        public static SearchModel FromTuple((string, string) tuple)
        {
            return new SearchModel
            {
                Group = tuple.Item1,
                Text = tuple.Item2
            };
        }

        public string Group { get; set; }

        public string Text { get; set; }
        
        public int DocId { get; set; }
    }
}
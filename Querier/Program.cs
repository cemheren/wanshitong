using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using wanshitong.Common.Lucene;

namespace Querier
{
    class Program
    {        
        private static LuceneTools m_luceneTools = new LuceneTools();

        static int Main(string[] args)
        {
            m_luceneTools.InitializeIndex();
            
            var rootCommand = new RootCommand();
            rootCommand.AddCommand(GetQueryCommand());

            return rootCommand.InvokeAsync(args).Result;
        }

        private static Command GetQueryCommand()
        {
            var listCommand = new Command("query");
            listCommand.Description = "Seek information";
            listCommand.Argument = new Argument<string>();

            listCommand.Handler = CommandHandler.Create<string>((query) =>
            {
                m_luceneTools.Search(query);
            });

            return listCommand;  
        }
    }
}

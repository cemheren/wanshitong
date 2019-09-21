using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using wanshitong.KeyAggregation;
using wanshitong.Common.Lucene;
using System.Linq;

namespace wanshitong
{
    class Program
    {
        public static KeyQueue KeyQueue = new KeyQueue(PrintQueue);

        private static ConcurrentDictionary<int, string> processIdMap = new ConcurrentDictionary<int, string>();

        private static LuceneTools m_luceneTools = new LuceneTools();

        static void Main(string[] args)
        {
            m_luceneTools.InitializeIndex();
            
            Task.Run(() => CreateProcessIdMapping());
            Task.Run(() => CreateKeyLoggerThread());
            Task.Run(() => RecurringPrinter());
            Task.Run(() => ClipboardListener());

            Task.Delay(Timeout.Infinite).Wait();
        }

        private static void RecurringPrinter()
        {
            while (true)
            {
                Task.Delay(10000).Wait();
                PrintQueue();
            }
        }

        private static string lastClipboard = ""; 
        private static void ClipboardListener()
        {
            while (true)
            {    
                var current = OsxClipboard.GetText();
                if(current != "" && current != lastClipboard)
                {
                    System.Console.WriteLine(current);
                    m_luceneTools.AddAndCommit("clipboard", current);
                    lastClipboard = current;
                }
                Task.Delay(2000).Wait();
                
            }
        }

        private static void PrintQueue()
        {
            var currentList = KeyQueue.Flush();
            if(currentList.Any()){
                foreach (var pair in currentList)
                {
                    System.Console.WriteLine($"{pair.Item1}, {pair.Item2}");
                    m_luceneTools.AddAndCommit(pair.Item1, pair.Item2);
                }
            }
        }

        private static void CreateProcessIdMapping()
        {
            while (true)
            {
                var allProcesses = Process.GetProcesses();
                foreach (var process in allProcesses)
                {
                    processIdMap.AddOrUpdate(process.Id, process.ProcessName, (a,b) => { return process.ProcessName; });
                }

                Task.Delay(60000).Wait();
            }
        }

        private static void CreateKeyLoggerThread()
        {
            var proc = new Process 
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "keylogger/keylogger",
                    Arguments = "",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                // do something with line 
                //System.Console.WriteLine(line);
                var key = line.Split(',')[0];
                var processId = line.Split(',')[1];
                processIdMap.TryGetValue(int.Parse(processId), out var processName);
                KeyQueue.Add(key, processName);
            }
        }
    }
}

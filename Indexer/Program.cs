using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using wanshitong.KeyAggregation;
using wanshitong.Common.Lucene;
using System.Linq;
using Microsoft.AspNetCore;
using Querier;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Keystroke.API;
using System.Runtime.InteropServices;

namespace wanshitong
{
    class Program
    {
        public static KeyQueue KeyQueue = new KeyQueue(PrintQueue);

        private static ConcurrentDictionary<int, string> processIdMap = new ConcurrentDictionary<int, string>();

        internal static LuceneTools m_luceneTools = new LuceneTools();

        static void Main(string[] args)
        {
            m_luceneTools.InitializeIndex();
            
            Task.Run(() => CreateProcessIdMapping());
            Task.Run(() => CreateKeyLoggerThread());
            Task.Run(() => RecurringPrinter());
            Task.Run(() => ClipboardListener());

            Task.Run(() => StartWebHost(args));

            Task.Delay(Timeout.Infinite).Wait();
        }

        private static void StartWebHost(string[] args)
        {
             var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

            host.Run();
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
                bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                string current;

                if(isWindows)
                {
                    current = WindowsClipboard.GetText();
                }else
                {
                    current = OsxClipboard.GetText();
                }

                if(!string.IsNullOrEmpty(current) && current != lastClipboard)
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
                    // filter noisy documents;
                    var spaces = pair.Item2.Split(' ', 10, StringSplitOptions.RemoveEmptyEntries);
                    if(spaces.Length < 2)
                        continue;

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
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                using (var api = new KeystrokeAPI())
                {
                    // Doesn't work on a VM might be fine on metal
                    
                    // api.CreateKeyboardHook((character) => 
                    // { 
                    //     //Console.Write(character); 
                    //     KeyQueue.Add(character.ToString(), character.CurrentWindow);
                    // });
                    Task.Delay(Timeout.Infinite).Wait();
                }
            }else
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
                    
                    if(int.TryParse(processId, out var pid) && processIdMap.TryGetValue(pid, out var processName))
                    {
                        KeyQueue.Add(key, processName);
                    }
                }
            }
        }
    }
}

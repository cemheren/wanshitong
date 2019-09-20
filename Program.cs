using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using wanshitong.KeyAggregation;

namespace wanshitong
{
    class Program
    {
        public static KeyQueue KeyQueue = new KeyQueue(PrintQueue);

        private static ConcurrentDictionary<int, string> processIdMap = new ConcurrentDictionary<int, string>();

        static void Main(string[] args)
        {
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
                    lastClipboard = current;
                }
                Task.Delay(2000).Wait();
                
            }
        }

        private static void PrintQueue()
        {
            var current = KeyQueue.Flush();
                if(current != "")
                    System.Console.WriteLine(current);
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

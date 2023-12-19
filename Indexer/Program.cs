using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using wanshitong.KeyAggregation;
using wanshitong.Common.Lucene;
using System.Linq;
using Microsoft.AspNetCore;
using Querier;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Configuration;
using OpenAI_API;

//using System.Windows.Forms;

namespace wanshitong
{
    class Program
    {
        private static ConcurrentDictionary<int, string> processIdMap = new ConcurrentDictionary<int, string>();

        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Console.CancelKeyPress += CurrentDomain_ProcessExit;
            AssemblyLoadContext.Default.Unloading += Default_Unloading;
            
            OCRClient.OcpKey = ConfigurationManager.AppSettings["ocrKey"];
            Telemetry.Version = args.Length > 0 ? args[0] : ConfigurationManager.AppSettings["version"];

            StartWebHost(args);
        }

        private static void Default_Unloading(AssemblyLoadContext obj)
        {
            Console.WriteLine("unload");
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("process exit");
            
            System.Threading.Thread.Sleep(1000);
            System.Environment.Exit(0);
        }

        protected static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            _closing.Set();
            System.Environment.Exit(0);
        }

        private static void StartWebHost(string[] args)
        {
             var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:4153")
                .ConfigureLogging((loggingBuilder) => 
                    {
                        loggingBuilder
                            .AddFilter<ConsoleLoggerProvider>(logLevel => logLevel >= LogLevel.Critical)
                            .AddConsole()
                            .AddDebug();
                    })
                .Build();

            host.Run();
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
    }
}

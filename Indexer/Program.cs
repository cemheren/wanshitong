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

using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

//using System.Windows.Forms;

namespace wanshitong
{
    class Program
    {
        public static KeyQueue KeyQueue = new KeyQueue(PrintQueue);

        private static ConcurrentDictionary<int, string> processIdMap = new ConcurrentDictionary<int, string>();

        internal static LuceneTools m_luceneTools = new LuceneTools();

        private static Process hotkeyCaptureProcess;

        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Console.CancelKeyPress += CurrentDomain_ProcessExit;
            AssemblyLoadContext.Default.Unloading += Default_Unloading;

            m_luceneTools.InitializeIndex();
            
            //Task.Run(() => CreateProcessIdMapping());
            Task.Run(() => CreateHotKeyThread());
            //Task.Run(() => RecurringPrinter());
            Task.Run(() => ClipboardListener());

            Task.Run(() => StartWebHost(args));

            Task.Delay(Timeout.Infinite).Wait();
        }

        private static void Default_Unloading(AssemblyLoadContext obj)
        {
            Console.WriteLine("unload");
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            hotkeyCaptureProcess.CloseMainWindow();
            hotkeyCaptureProcess.Close();
            //hotkeyCaptureProcess.Kill();
            
            Console.WriteLine("process exit");
            System.Environment.Exit(0);
        }

        protected static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Exit");
            hotkeyCaptureProcess.CloseMainWindow();
            hotkeyCaptureProcess.Close();
            //hotkeyCaptureProcess.Kill();

            _closing.Set();
            System.Environment.Exit(0);
        }

        private static void StartWebHost(string[] args)
        {
             var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
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

        private static void RecurringPrinter()
        {
            while (true)
            {
                Task.Delay(60000).Wait();
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
                    m_luceneTools.AddAndCommit("clipboard", current, -1);
                    lastClipboard = current;
                }

                Task.Delay(2000).Wait();
                
            }
        }

        private static void PrintQueue()
        {
             var currentList = KeyQueue.Flush();
             {
                foreach (var pair in currentList)
                {
                    // filter noisy documents;
                    // var spaces = pair.Item2.Split(' ', 10, StringSplitOptions.RemoveEmptyEntries);
                    // if(spaces.Length < 2)
                    //     continue;

                    processIdMap.TryGetValue(pair.Item1, out var processName);

                    var existing = m_luceneTools.SearchWithProcessId(pair.Item1, processName);

                    if(existing != null)
                    {
                        existing.Text += "\n" + pair.Item2;
                        m_luceneTools.UpdateDocument(existing);    
                    }
                    else
                    {
                        m_luceneTools.AddAndCommit(processName, pair.Item2, pair.Item1);
                    }
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

        private static void CreateHotKeyThread()
        {
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                hotkeyCaptureProcess = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "Windows/Hotkey/KeyLogger",
                        Arguments = "",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = false
                    });
                
                Task.Run(() => {
                    hotkeyCaptureProcess.WaitForExit();
                });

                while (!hotkeyCaptureProcess.StandardOutput.EndOfStream)
                {
                    string line = hotkeyCaptureProcess.StandardOutput.ReadLine();
                    System.Console.WriteLine("capture event");
                    if (line == "alt-A")
                    {
                        var image = ScreenCapture.CaptureActiveWindow();
                        MemoryStream memoryStream = new MemoryStream();
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                        var currentDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        var dirInfo = new DirectoryInfo(Path.Combine(currentDir, "Screenshots"));
                        dirInfo.Create();

                        try
                        {
                            var name = Guid.NewGuid();
                            var address = $@"{dirInfo}\{name}.jpeg";
                            image.Save(address, ImageFormat.Jpeg);
                        
                            var result = OCRClient.MakeRequest(memoryStream.ToArray()).Result;
                            var str = result.GetString();
                            m_luceneTools.AddAndCommit(address, str, -10);
                            System.Console.WriteLine("capture done...");
                        }
                        catch (System.Exception e)
                        {
                            System.Console.WriteLine("capture failed...");
                            System.Console.WriteLine(e.Message);
                        }
                    }
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
                    var keyboardKey = line.Split(',')[0];
                    var processId = line.Split(',')[1];
                    
                    if(int.TryParse(processId, out var pid))
                    {
                        KeyQueue.Add(keyboardKey, pid);
                    }
                }
            }
        }
    }
}

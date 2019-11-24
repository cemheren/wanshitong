using System.Web.Http;
using System.Collections.Generic;
using wanshitong;
using System.Linq;
using Indexer.LuceneTools;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.DataContracts;

namespace Indexer.Querier.Controllers
{
 

    public class ActionsController : ApiController
    {
        [HttpGet]
        public bool Screenshot()
        {
            Telemetry.Instance.TrackEvent("ActionsController.Screenshot");

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
                Program.m_luceneTools.AddAndCommit(address, str, -10);
                System.Console.WriteLine("capture done...");

                Telemetry.Instance.TrackTrace("Capture.Success", SeverityLevel.Verbose);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("capture failed...");
                System.Console.WriteLine(e.Message);

                Telemetry.Instance.TrackTrace("Capture.Error", SeverityLevel.Error);
                Telemetry.Instance.TrackException(e);

                return false;
            }

            return true;
        }

        private static string lastClipboard;

        [HttpGet]
        public bool CopyClipboard()
        {
            Telemetry.Instance.TrackEvent("ActionsController.CopyClipboard");

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
                Program.m_luceneTools.AddAndCommit("clipboard", current, -1);
                lastClipboard = current;

                var d = new Dictionary<string, string>();
                d.Add("content", current);
                Telemetry.Instance.TrackTrace("CopyClipboard", severityLevel: SeverityLevel.Verbose, properties: d);
            }

            return true;
        }
    }
}
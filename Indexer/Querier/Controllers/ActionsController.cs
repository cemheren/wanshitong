using System.Web.Http;
using System.Collections.Generic;
using wanshitong;
using System.Linq;
using Indexer.LuceneTools;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Indexer.Querier.Controllers
{
 

    public class ActionsController : ApiController
    {
        [HttpGet]
        public bool Screenshot()
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
                Program.m_luceneTools.AddAndCommit(address, str, -10);
                System.Console.WriteLine("capture done...");
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("capture failed...");
                System.Console.WriteLine(e.Message);
            
                return false;
            }

            return true;
        }

        private static string lastClipboard;

        [HttpGet]
        public bool CopyClipboard()
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
                Program.m_luceneTools.AddAndCommit("clipboard", current, -1);
                lastClipboard = current;
            }

            return true;
        }
    }
}
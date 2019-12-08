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
using System.Threading.Tasks;
using System.Drawing;

namespace Indexer.Querier.Controllers
{
    public class ActionsController : ApiController
    {
        [HttpGet]
        public bool Screenshot()
        {
            Telemetry.Instance.TrackEvent("ActionsController.Screenshot");
            
            try
            {
                Validation.PremiumOrUnderLimit();

                bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            
                var currentDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var screenshotsPath = Path.Combine(currentDir, "Screenshots");
                var dirInfo = new DirectoryInfo(screenshotsPath);
                dirInfo.Create();
                var name = Guid.NewGuid();
                var address = Path.Combine(screenshotsPath, $"{name}.jpeg");
                
                OcrResponse result;
                if(isWindows){
                    var image = ScreenCapture.CaptureActiveWindow();
                    
                    MemoryStream memoryStream = new MemoryStream();
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    image.Save(address, ImageFormat.Jpeg);

                    result = OCRClient.MakeRequest(memoryStream.ToArray()).Result;
                }
                else
                {
                    ScreenCapture.CaptureScreen(address);
                    byte[] imgData = System.IO.File.ReadAllBytes(address);
                
                    result = OCRClient.MakeRequest(imgData).Result;
                }
            
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

            try
            {
                Validation.PremiumOrUnderLimit();

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
            }
            catch (System.Exception e)
            {
                Telemetry.Instance.TrackException(e);
                return false;
            }

            return true;
        }

        [HttpGet]
        public async Task<bool> SetPremiumKey(string key)
        {
            Telemetry.Instance.TrackEvent("ActionsController.SetPremiumKey");

            try
            {
                await Validation.ValidatePremiumKey(key);

                Storage.Instance.Store("PremiumKey", key);        
                Storage.Instance.Persist();       
            }
            catch (System.Exception e)
            {
                Telemetry.Instance.TrackException(e);
                throw;
            }

            return true;
        }

        [HttpGet]
        public string GetPremiumKey()
        {
            Telemetry.Instance.TrackEvent("ActionsController.GetPremiumKey");

            try
            {
                return Storage.Instance.Get<string>("PremiumKey");                
            }
            catch (System.Exception e)
            {
                Telemetry.Instance.TrackException(e);
            }

            return null;
        }

        [HttpPost]
        public bool EditText(string myId, [Microsoft.AspNetCore.Mvc.FromBody]string newText)
        {
            Telemetry.Instance.TrackEvent("ActionsController.EditText");
            try
            {
                
                var currentDocument = Program
                    .m_luceneTools
                    .SearchWithMyId(myId);

                currentDocument.Text = newText;

                Program
                    .m_luceneTools
                    .UpdateDocument(currentDocument, true);

            }
            catch (System.Exception e)
            {
                Telemetry.Instance.TrackException(e);
                return false;
            }

            return true;
        }

        [HttpPost]
        public bool IngestCroppedDocument(string myId, [Microsoft.AspNetCore.Mvc.FromBody]CropImageModel model)
        {
            Telemetry.Instance.TrackEvent("ActionsController.IngestCroppedDocument");
            try
            {
                var currentDocument = Program
                    .m_luceneTools
                    .SearchWithMyId(myId);

                string fileName = currentDocument.Group;
                var source = new Bitmap(fileName);

                var cropped = source.Crop(model);
                
                var currentDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var screenshotsPath = Path.Combine(currentDir, "Screenshots");
                var dirInfo = new DirectoryInfo(screenshotsPath);
                dirInfo.Create();
            
                var name = Guid.NewGuid();
                var address = Path.Combine(screenshotsPath, $"{name}.jpeg");
                cropped.Save(address, ImageFormat.Jpeg);
            
                var result = OCRClient.MakeRequest(OCRClient.BitmapToByteArray(cropped)).Result;
                var str = result.GetString();

                currentDocument.Type = "CroppedDocument";
                currentDocument.Text = str;
                currentDocument.Group = address;
                currentDocument.MyId = Guid.NewGuid().ToString();

                Program
                    .m_luceneTools
                    .UpdateDocument(currentDocument, false);

            }
            catch (System.Exception e)
            {
                Telemetry.Instance.TrackException(e);
                return false;
            }

            return true;
        }
    }
}
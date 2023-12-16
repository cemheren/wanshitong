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
using System.Windows.Forms;
using System.Configuration;

namespace Indexer.Querier.Controllers
{
    public class ActionsController : ApiController
    {
        public ActionsController(Storage storage, Telemetry telemetry, OpenAIWrapper openAIWrapper)
        {
            this.storage = storage;
            this.telemetry = telemetry;
            this.openAIWrapper = openAIWrapper;

        }

        private static string ScreenshotDir {
        get
            {
                var rootDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if(ConfigurationManager.AppSettings["rootFolderPath"] != null)
                {
                    rootDir = ConfigurationManager.AppSettings["rootFolderPath"];
                }
                var screenshotsPath = Path.Combine(rootDir, "Screenshots");
                var dirInfo = new DirectoryInfo(screenshotsPath);
                dirInfo.Create();
                return screenshotsPath;
            }
        }

        [HttpGet]
        public bool Screenshot()
        {
            this.telemetry.client.TrackEvent("ActionsController.Screenshot");
            
            try
            {
                //Validation.PremiumOrUnderLimit();

                bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            
                var name = Guid.NewGuid();
                var address = Path.Combine(ScreenshotDir, $"{name}.jpeg");
                
                OcrResponse result;
                if(isWindows){
                    var image = ScreenCapture.CaptureActiveWindow();
                    
                    MemoryStream memoryStream = new MemoryStream();
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    image.Save(address, ImageFormat.Jpeg);

                    var imageBytes = memoryStream.ToArray();
                    this.openAIWrapper.TestVision(imageBytes).Wait();

                    result = OCRClient.MakeRequest(imageBytes).Result;
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

                this.telemetry.client.TrackTrace("Capture.Success", SeverityLevel.Verbose);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("capture failed...");
                System.Console.WriteLine(e.Message);

                this.telemetry.client.TrackTrace("Capture.Error", SeverityLevel.Error);
                this.telemetry.client.TrackException(e);

                return false;
            }

            return true;
        }

        private static string lastClipboard;
        private readonly Storage storage;
        private readonly Telemetry telemetry;
        private readonly OpenAIWrapper openAIWrapper;


        [HttpGet]
        public bool CopyClipboard()
        {
            this.telemetry.client.TrackEvent("ActionsController.CopyClipboard");

            try
            {
                //Validation.PremiumOrUnderLimit();

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
                    this.telemetry.client.TrackTrace("CopyClipboard", severityLevel: SeverityLevel.Verbose, properties: d);
                }
            }
            catch (System.Exception e)
            {
                this.telemetry.client.TrackException(e);
                return false;
            }

            return true;
        }

        [HttpGet]
        public async Task<bool> SetPremiumKey(string key)
        {
            this.telemetry.client.TrackEvent("ActionsController.SetPremiumKey");

            try
            {
                // await Validation.ValidatePremiumKey(key);

                this.storage.Instance.Store("PremiumKey", key);        
                this.storage.Instance.Persist();       
            }
            catch (System.Exception e)
            {
                this.telemetry.client.TrackException(e);
                throw;
            }

            return true;
        }

        [HttpGet]
        public string GetPremiumKey()
        {
            this.telemetry.client.TrackEvent("ActionsController.GetPremiumKey");

            try
            {
                return this.storage.Instance.Get<string>("PremiumKey");                
            }
            catch (System.Exception e)
            {
                this.telemetry.client.TrackException(e);
            }

            return null;
        }

        [HttpGet]
        public dynamic GetStats()
        {
            this.telemetry.client.TrackEvent("ActionsController.GetStats");

            try
            {
                return new {
                    docCount = this.storage.GetOrDefault("maxdoc", 0)
                };
            }
            catch (System.Exception e)
            {
                this.telemetry.client.TrackException(e);
            }

            return null;
        }

        [HttpPost]
        public bool EditText(string myId, [Microsoft.AspNetCore.Mvc.FromBody]string newText)
        {
            this.telemetry.client.TrackEvent("ActionsController.EditText");
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
                this.telemetry.client.TrackException(e);
                return false;
            }

            return true;
        }

        [HttpPost]
        public bool IngestCroppedDocument(string myId, [Microsoft.AspNetCore.Mvc.FromBody]CropImageModel model)
        {
            this.telemetry.client.TrackEvent("ActionsController.IngestCroppedDocument");
            try
            {
                var currentDocument = Program
                    .m_luceneTools
                    .SearchWithMyId(myId);

                string fileName = currentDocument.Group;
                var source = new Bitmap(fileName);

                var cropped = source.Crop(model);
                
                var name = Guid.NewGuid();
                var address = Path.Combine(ScreenshotDir, $"{name}.jpeg");
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
                this.telemetry.client.TrackException(e);
                return false;
            }

            return true;
        }
    }
}
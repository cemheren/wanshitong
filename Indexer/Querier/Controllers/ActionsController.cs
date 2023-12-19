using System.Web.Http;
using System.Collections.Generic;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.DataContracts;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using wanshitong.Common.Lucene;
using System.Linq;

namespace Indexer.Querier.Controllers
{
    public class ActionsController : ApiController
    {
        private static string lastClipboard;
        private readonly Storage storage;
        private readonly Telemetry telemetry;
        private readonly OpenAIWrapper openAIWrapper;
        private readonly wanshitong.Common.Lucene.LuceneClient luceneClient;
        private readonly OCRClient OCRClient;

        private readonly string screenShotDir;

        public ActionsController(Storage storage, Telemetry telemetry, OpenAIWrapper openAIWrapper, LuceneClient luceneClient, OCRClient OCRClient)
        {
            this.storage = storage;
            this.telemetry = telemetry;
            this.openAIWrapper = openAIWrapper;
            this.luceneClient = luceneClient;
            this.OCRClient = OCRClient;
            this.screenShotDir = Path.Combine((string)storage.Instance.Get("rootFolderPath"), "Screenshots");
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
                var address = Path.Combine(this.screenShotDir, $"{name}.jpeg");
                
                OpenAIModel openAIModel = null;
                OcrResponse result;
                if(isWindows){
                    var image = ScreenCapture.CaptureActiveWindow();
                    
                    MemoryStream memoryStream = new MemoryStream();
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    image.Save(address, ImageFormat.Jpeg);

                    var imageBytes = memoryStream.ToArray();
                    openAIModel = this.openAIWrapper.ExamineScreenShot(imageBytes).Result;

                    result = this.OCRClient.MakeRequest(imageBytes).Result;
                }
                else
                {
                    ScreenCapture.CaptureScreen(address);
                    byte[] imgData = System.IO.File.ReadAllBytes(address);
                
                    openAIModel = this.openAIWrapper.ExamineScreenShot(imgData).Result;
                    result = this.OCRClient.MakeRequest(imgData).Result;
                }
            
                var str = $"{openAIModel?.Description}\n\n{result.GetString()}";;
                var tags = openAIModel.Classification;
                this.luceneClient.AddAndCommit(address, str, -10, tags: [tags]);
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

                    var openAiResponse = this.openAIWrapper.ExamineStringContent(current).Result;

                    var str = $"{openAiResponse.Description}\n{current}";
                    var tags = openAiResponse.Classification;

                    this.luceneClient.AddAndCommit("clipboard", str, -1, [tags]);
                    lastClipboard = current;

                    var d = new Dictionary<string, string>
                    {
                        { "content", current }
                    };
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
                
                var currentDocument = this.luceneClient.SearchWithMyId(myId);
                currentDocument.Text = newText;
                
                this.luceneClient.UpdateDocument(currentDocument, true);

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
                var currentDocument = this.luceneClient.SearchWithMyId(myId);

                string fileName = currentDocument.Group;
                var source = new Bitmap(fileName);

                var cropped = source.Crop(model);
                
                var name = Guid.NewGuid();
                var address = Path.Combine(this.screenShotDir, $"{name}.jpeg");
                cropped.Save(address, ImageFormat.Jpeg);
            
                var byteArray = this.OCRClient.BitmapToByteArray(cropped);
                var result = this.OCRClient.MakeRequest(byteArray).Result;
                var openAIModel = this.openAIWrapper.ExamineScreenShot(byteArray).Result;
                
                var str = $"{openAIModel?.Description}\n\n{result.GetString()}";;
                var tags = openAIModel.Classification;
                this.luceneClient.AddAndCommit(address, str, -10, tags: [tags]);

                var existingTags = new HashSet<string>(currentDocument.Tags);
                existingTags.Add(tags);

                currentDocument.Tags = existingTags.ToArray();
                currentDocument.Type = "CroppedDocument";
                currentDocument.Text = str;
                currentDocument.Group = address;
                currentDocument.MyId = Guid.NewGuid().ToString();

                this.luceneClient.UpdateDocument(currentDocument, false);

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
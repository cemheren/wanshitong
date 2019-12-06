using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

public class OCRClient
{
    public static async Task<OcrResponse> MakeRequest(byte[] byteData)
    {
        var client = new HttpClient();
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        // Request headers
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "d9707c0b9f544cb1b974caff2041a589");

        // Request parameters
        queryString["language"] = "unk";
        queryString["detectOrientation "] = "true";
        var uri = "https://ocrdemowst.cognitiveservices.azure.com/vision/v1.0/ocr?" + queryString;

        HttpResponseMessage response;

        // Request body
        //byte[] byteData = BitmapToByteArray(image);

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response = await client.PostAsync(uri, content);
        }

        return await response.Content.ReadAsAsync<OcrResponse>();
    }

    public static byte[] BitmapToByteArray(Bitmap bitmap)
    {
        using (var stream = new MemoryStream())
        {
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }
}
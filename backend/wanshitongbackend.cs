using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace wanshitong.backend
{
    public static class wanshitongbackend
    {
        [FunctionName("wanshitongbackend")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string api = req.Query["api"];

            switch (api)
            {
                case "newkey":
                    return await HandleNewKey(req);
                default:
                    return await HandleNotFound(api);
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var name = data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private static async Task<IActionResult> HandleNewKey(HttpRequest req)
        {

            
            return (ActionResult)new OkObjectResult($"{Guid.NewGuid()}");
        }

        private static async Task<IActionResult> HandleNotFound(string route)
        {
            return new NotFoundObjectResult($"Route {route} is not found");
        }
    }
}

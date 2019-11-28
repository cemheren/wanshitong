using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using wanshitong.backend.datalayer;

namespace wanshitong.backend
{
    public static class wanshitongbackend
    {
        private static KeyDataLayer keyDataLayer = new KeyDataLayer();

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
                    return await HandleNewKey(req, log);
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

        private static async Task<IActionResult> HandleNewKey(HttpRequest req, ILogger log)
        {
            string key;
            try
            {
                key = await keyDataLayer.CreateNewValidKey("empty", log);
            }
            catch (System.Exception e)
            {
                log.LogError(e, "Key generation failed");
                return (ActionResult)new BadRequestObjectResult(
                    new ErrorModel{
                        Message = "Key generation failed"
                    });
            }

            try
            {
                StripeConfiguration.ApiKey = "sk_test_Oir6aZatDyqBDcTg4n2smYB5001MmaUfRW";
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<StripeTokenModel>(requestBody);

                var options = new ChargeCreateOptions {
                    Source = data?.token,
                    Currency = "USD",
                    Amount = 4000
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);
            }
            catch (System.Exception e)
            {
                // todo: revert key to invalid here. 

                log.LogError(e, "Stripe authorization failed");
                return (ActionResult)new BadRequestObjectResult(
                    new ErrorModel{
                        Message = "Stripe authorization failed"
                    });
            }

            return (ActionResult)new OkObjectResult($"{key}");
        }

        private static async Task<IActionResult> HandleNotFound(string route)
        {
            return new NotFoundObjectResult($"Route {route} is not found");
        }
    }
}

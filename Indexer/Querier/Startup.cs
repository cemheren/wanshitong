using System.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI_API;

namespace Querier
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            var api = new OpenAI_API.OpenAIAPI(
                new APIAuthentication(
                    ConfigurationManager.AppSettings["openAI_key"], 
                    ConfigurationManager.AppSettings["openAI_org"]));
            var wrapper = new OpenAIWrapper(api);

            var storage = new Storage();
            services.AddSingleton<OpenAIWrapper>(wrapper);
            services.AddSingleton<Storage>(storage);
            services.AddSingleton<Telemetry>(new Telemetry(storage));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Screenshot",
                    template: "actions/screenshot",
                    defaults: new { controller = "Actions", action = "Screenshot" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "Clipboard",
                    template: "actions/clipboard",
                    defaults: new { controller = "Actions", action = "CopyClipboard" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "GetStats",
                    template: "actions/getstats",
                    defaults: new { controller = "Actions", action = "GetStats" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "GetPremiumKey",
                    template: "actions/getpremiumkey",
                    defaults: new { controller = "Actions", action = "GetPremiumKey" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "SetPremiumKey",
                    template: "actions/setpremiumkey/{key}",
                    defaults: new { controller = "Actions", action = "SetPremiumKey" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "EditText",
                    template: "actions/edittext/{myId}",
                    defaults: new { controller = "Actions", action = "EditText" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Post.ToString()) });

                routes.MapRoute(
                    name: "IngestCroppedDocument",
                    template: "actions/ingestcroppeddocument/{myId}",
                    defaults: new { controller = "Actions", action = "IngestCroppedDocument" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Post.ToString()) });

                routes.MapRoute(
                    name: "ExecuteQuery",
                    template: "query/{text}",
                    defaults: new { controller = "Query", action = "SearchText" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "ExecuteQuery",
                    template: "timerange/{start}/{end}",
                    defaults: new { controller = "Query", action = "SearchWithDates" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "GetAll",
                    template: "getAll",
                    defaults: new { controller = "Query", action = "GetAll" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "DeleteDocument",
                    template: "delete/{docId}",
                    defaults: new { controller = "Query", action = "Delete" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Delete.ToString()) });
                    
                routes.MapRoute(
                    name: "TagDocuments",
                    template: "tag",
                    defaults: new { controller = "Query", action = "TagDocs" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Post.ToString()) });
                    
                routes.MapRoute(
                    name: "SearchTag",
                    template: "tags/{text}",
                    defaults: new { controller = "Tags", action = "Search" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });
                    
                routes.MapRoute(
                    name: "AddTag",
                    template: "addTag/{tag}/{color}",
                    defaults: new { controller = "Tags", action = "AddTag" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                    
                routes.MapRoute(
                    name: "AddSavedSearch",
                    template: "savedsearch/add/{phrase}",
                    defaults: new { controller = "SavedSearch", action = "AddSavedSearch" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "GetSavedSearches",
                    template: "savedsearch/get",
                    defaults: new { controller = "SavedSearch", action = "GetSavedSearches" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

            });
        }
    }
}
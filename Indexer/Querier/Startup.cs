using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                    name: "ExecuteQuery",
                    template: "query/{text}",
                    defaults: new { controller = "Query", action = "SearchText" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });

                routes.MapRoute(
                    name: "GetAll",
                    template: "getAll",
                    defaults: new { controller = "Query", action = "GetAll" },
                    constraints: new { httpMethod = new HttpMethodRouteConstraint(HttpMethod.Get.ToString()) });
            });
        }
    }
}
using Enrich.Common.Enums;
using Enrich.Core.Infrastructure;
using Enrich.IMS.RestApi.Library.Filters;
using Enrich.IMS.RestApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Enrich.IMS.RestApi.Library.Infrastructure
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configure the application HTTP request pipeline
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        /// <param name=""></param>      
        public static void ConfigureRequestPipeline(this IApplicationBuilder application)
        {
#if DEBUG
            application.UseDeveloperExceptionPage();
#endif

            application.UseSwagger();

            application.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Enrich IMS API");
                c.RoutePrefix = string.Empty;
            });

            application.UseRouting();
            application.UseCors();

            // middlewares
            application.UseMiddleware<RequestOptionsMiddleware>();

            application.UseAuthentication();
            application.UseAuthorization();
            application.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //find startup configurations provided by other assemblies
            var typeFinder = Singleton<ITypeFinder>.Instance;
            var startupConfigurations = typeFinder.FindClassesOfType<IEnrichStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                .Select(startup => (IEnrichStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            //configure request pipeline
            foreach (var instance in instances)
                instance.Configure(application);


        }
    }
}

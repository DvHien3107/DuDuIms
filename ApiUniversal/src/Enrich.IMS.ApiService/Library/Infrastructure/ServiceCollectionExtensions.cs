using Enrich.Common.Enums;
using Enrich.IMS.RestApi.Library.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace Enrich.IMS.RestApi.Library.Infrastructure
{
    /// <summary>
    /// Represents extensions of IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Add services to the application and configure service provider
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
      
        public static void ConfigureApplicationServices(this IServiceCollection services)
        {
            //let the operating system decide what TLS protocol version to use
            //see https://docs.microsoft.com/dotnet/framework/network-programming/tls
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            services.AddControllers();
            services.Configure<GzipCompressionProviderOptions>(opt => opt.Level = CompressionLevel.Optimal);
            services.AddResponseCompression(opt =>
            {
                opt.Providers.Add<GzipCompressionProvider>();
                opt.MimeTypes = new[] { "application/json", "application/json; charset=utf-8" };
                opt.EnableForHttps = true;
            })
            .AddCors(policyBuilder =>
                policyBuilder.AddDefaultPolicy(policy =>
                    policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod()))
            .AddResponseCaching()
            .AddMvc(opt =>
            {
                opt.Filters.Add<EnrichAuthorizationFilter>();
                opt.Filters.Add<EnrichExceptionFilter>();
                opt.Filters.Add<EnrichResultFilter>();
                
            })
            //.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .ConfigureApiBehaviorOptions(opt =>
            {
                // https://docs.microsoft.com/en-us/aspnet/core/web-api/index?view=aspnetcore-2.2#automatic-http-400-responses
                opt.SuppressModelStateInvalidFilter = true; // will handle in ModelStateValidationActionFilter
            })
            //.AddNewtonsoftJson(opt =>
            //{
            //    opt.SerializerSettings.Formatting = Formatting.None;
            //    opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            //    opt.SerializerSettings.ContractResolver = new DefaultContractResolver(); //CamelCasePropertyNamesContractResolver
            //                                                                             //opt.SerializerSettings.Converters.Add(new Library.JsonConverters.AsyncEnumerableJsonConverter());
            //})
            ;
            services.AddHttpClient();

            services.Configure<FormOptions>(c =>
            {
                c.KeyLengthLimit = int.MaxValue;
                c.ValueLengthLimit = int.MaxValue;
                c.BufferBodyLengthLimit = int.MaxValue;
                c.MultipartBodyLengthLimit = int.MaxValue;
                c.MultipartHeadersLengthLimit = int.MaxValue;
                c.MemoryBufferThreshold = int.MaxValue;
            });

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
                var description = "This is REST API using for Enrich-IMS";
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "1.1.0",
                    Title = "Enrich IMS WebAPI",
                    Description = description,
                    //TermsOfService = "None",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact { Name = "Quoc Do", Email = "quoc.do@enrichco.us", Url = new Uri("https://enrichco.us/") }
                });

                c.OperationFilter<Enrich.IMS.RestApi.Library.Swagger.AddAuthHeader>();
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First()); 

            });

        }
    }
}

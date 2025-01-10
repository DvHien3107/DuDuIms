using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Core;
using Enrich.Core.Configs;
using Enrich.Core.Container;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.IMS.Infrastructure.Data.Implement.Repositories;
using Enrich.IMS.Services.Implement.Library;
using Enrich.IMS.Services.Implement.Mappers;
using Enrich.IMS.Services.Implement.Services;
using Enrich.IMS.Services.Implement.Validation;
using Enrich.Infrastructure.Cache;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using Enrich.Infrastructure.Email;
using Enrich.Infrastructure.IoC;
using Enrich.Infrastructure.Log;
using Enrich.Infrastructure.Slack;
using Enrich.Payment.MxMerchant;
using Enrich.Payment.MxMerchant.Api;
using Enrich.RestApi.NetCorePlatform.Auth;
using Enrich.RestApi.NetCorePlatform.Auth.Basic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform
{
    public class EnrichWebApiStartup : IEnrichStartup
    {
        public int Order => 1;

        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<ActivatorService>().InstancePerLifetimeScope();
            builder.Register(c => new HttpClient()).As<HttpClient>();
            builder.LoadConfig(configuration);
        }

        public void Configure(IApplicationBuilder application)
        {

        }
    }
}

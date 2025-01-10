using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Core;
using Enrich.Core.Configs;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.IMS.Services.Implement.Library;
using Enrich.IMS.Services.Implement.Mappers;
using Enrich.Infrastructure.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Infrastructure
{
    public class EnrichAutoMapperStartup : IEnrichStartup
    {
      
        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {

            //Mappers for IMS
            builder.RegisterAssemblyTypes(Assembly<MemberMapper>())
                .Where(t => t.Name.EndsWith("Mapper"))
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        public void Configure(IApplicationBuilder application)
        {

        }

        #region method      

        private static Assembly Assembly<T>()
        {
            var result = typeof(T).GetTypeInfo().Assembly;
            return result;
        }
        #endregion
    }
}

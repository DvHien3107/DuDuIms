using Autofac;
using Enrich.IMS.Dto.Authentication;
using Enrich.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace Enrich.RestApi.NetCorePlatform
{
    public static class ConfigManagerExtensions
    {
        /// <summary>
        /// load connection of servers e.g redis, mongo, sql
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>

        public static EnrichConectionString LoadConfig(this ContainerBuilder builder, IConfiguration configure)
        {
            var config = new EnrichConectionString();
            LoadConnectionConfig(config, configure);
            builder.Register<EnrichConectionString>(b => config).SingleInstance();
            builder.RegisterInstance<JwtSettings>(configure.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings()).SingleInstance();          
            return config;
        }

        /// <summary>
        /// load base
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="configure"></param>
        private static void LoadConnectionConfig(EnrichConectionString config, IConfiguration configure)
        {
            if (config == null)
            {
                config = new EnrichConectionString();
            }
            configure.GetSection("ConnectionStrings").Bind(config);
        }
    }
}

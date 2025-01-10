using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Auth
{
    public class AuthData
    {
        private static IConfigurationSection _configuration;
        public static void Configure(IConfigurationSection configuration)
        {
            _configuration = configuration;
        }

        public static string Key => _configuration["Key"];
        public static string Issuer => _configuration["Issuer"];
        public static string Audience => _configuration["Audience"];
    }
}

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Comon
{
    public class SettingData
    {
        private static IConfigurationSection _configuration;
        public static void Configure(IConfigurationSection configuration)
        {
            _configuration = configuration;
        }
        public static string EMAIL_SENDER => _configuration["EmailSender"];
        public static string EMAIL_PASSWORD => _configuration["EmailPass"];
        public static string EMAIL_SMTP => _configuration["EmailSmtp"];
    }
}

using Enrich.Dto.Base.POSApi;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Drawing2D;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services.NextGen.IBO
{
    public interface IIboSyncService
    {
         Task<Object> CallHookSyncClickup(string CustomerCode);
    }
    public class IboSyncService : IIboSyncService
    {
        private readonly string _domainIms;
        public IboSyncService(IConfiguration appConfig)
        {
            _domainIms = appConfig["IMSUrl:Domain"];
        }

        public async Task<Object> CallHookSyncClickup(string CustomerCode)
        {
            string apiUrl = _domainIms +  "/MerchantMan/HookSynsClickUp";
            object valueObject = new {
                CustomerCode = CustomerCode
            };
            HttpClient client = new HttpClient();
            // Add an Accept header for JSON format.
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.PostAsJsonAsync(apiUrl, valueObject).Result;
            var data = await response.Content.ReadAsStringAsync();
            var table = JsonConvert.DeserializeObject<Object>(data);
            return new {
                data = data,
                table = table,
            };
        }

    }
}

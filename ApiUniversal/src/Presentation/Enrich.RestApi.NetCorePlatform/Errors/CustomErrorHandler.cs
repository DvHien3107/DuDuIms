using Enrich.Core;
using Enrich.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Errors
{
    public static class CustomErrorHandler
    {
        public static async Task PostBugAsync(IEnrichSlack postBug, Exception exception, string extendMessage = "")
        {
            try
            {
                var suffixMessage = !string.IsNullOrWhiteSpace(extendMessage) ? $"{Environment.NewLine}{extendMessage}" : string.Empty;
                await postBug.PostAsync(exception, suffixMessage: suffixMessage);
            }
            catch { }
        }

        public static void LogError(IEnrichLog log, Exception exception, string extendMessage = "")
        {
            log.Error(exception, !string.IsNullOrWhiteSpace(extendMessage) ? $"ERROR {extendMessage}" : "An exception occured during processing a request");
        }

        public static string ToJson(object obj) => obj != null
            ? JsonConvert.SerializeObject(obj, new JsonSerializerSettings { Formatting = Formatting.None, Error = (s, a) => a.ErrorContext.Handled = true })
            : string.Empty;
    }
}

using Enrich.Common;
using Enrich.IMS.Dto.PostBug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Utils
{
    public interface IEnrichSlack
    {      

        Task<SlackPostMessageResponse> PostAsync(Exception exception, string prefixMessage = "", string suffixMessage = "",
            string[] attFiles = null, string errorDataFile = "");

        Task<SlackPostMessageResponse> PostAsync(string message, string[] attFiles = null, string errorDataFile = "");

        Task<SlackPostMessageResponse> PostIntokChannelAsync(string slackChannel, string message);
    }
}

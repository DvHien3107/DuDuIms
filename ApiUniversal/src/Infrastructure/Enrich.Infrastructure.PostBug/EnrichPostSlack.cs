using Enrich.Core.Utils;
using Enrich.IMS.Dto.PostBug;
using SlackAPI;
using System;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Slack
{
    public class EnrichSlack : IEnrichSlack
    {
        private readonly SlackTaskClient _slackTaskClient;
        public EnrichSlack(string token)
        {
            _slackTaskClient = new SlackTaskClient(token);
        }

        public Task<SlackPostMessageResponse> PostAsync(Exception exception, string prefixMessage = "", string suffixMessage = "", string[] attFiles = null, string errorDataFile = "")
        {
            throw new NotImplementedException();
        }

        public async Task<SlackPostMessageResponse> PostAsync(string message, string[] attFiles = null, string errorDataFile = "")
        {
            throw new NotImplementedException();
        }

        public async Task<PostMessageResponse> PostIntokChannelAsync(string slackChannel, string message)
        {
            var response = await _slackTaskClient.PostMessageAsync(slackChannel, message);
            return (PostMessageResponse)response;
        }
        
        Task<SlackPostMessageResponse> IEnrichSlack.PostIntokChannelAsync(string slackChannel, string message)
        {
            throw new NotImplementedException();
        }
    }
}


using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.IMS.Dto.Authentication;
using Enrich.RestApi.NetCorePlatform.Auth;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform
{
    public sealed class OAuthAuthProvider : IAuthProvider
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEnrichContextFactory _contextFactory;
        private readonly IConfiguration _appConfig;
        private readonly string _userProfileUrl;
        public string Name => AuthMode.OAuth.ToString();

        public EnrichContext Context { get; }


        public OAuthAuthProvider(IEnrichContextFactory contextFactory, EnrichContext context, IHttpClientFactory httpClientFactory, IConfiguration appConfig)
        {
            Context = context;
            _contextFactory = contextFactory;
            _httpClientFactory = httpClientFactory;
            _userProfileUrl = appConfig["AuthSettings:OAuth:UserProfile"];
        }

        public async Task<(AuthStatus Status, EnrichContext Context, ClaimsPrincipal User)> ValidateAsync(params (string Key, string Value)[] values)
        {
            var token = values.FirstOrDefault(a => a.Key == "Parameter").Value ?? string.Empty;
           
            if (string.IsNullOrWhiteSpace(token))
            {
                return (AuthStatus.Unauthorized, Context, null);
            }

            try
            {
                var userProfile = new UserProfile();
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    using (var response = await httpClient.GetAsync(_userProfileUrl).ConfigureAwait(false))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                            userProfile = JsonConvert.DeserializeObject<UserProfile>(payloadString);
                            
                          
                        }
                        else
                        {
                            var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                            var payload = JsonConvert.DeserializeObject<Response>(payloadString);

                            if (payload.Code == HttpStatusCode.Unauthorized)
                                throw new UnauthorizedAccessException(payload.Message);

                            throw new HttpResponseException(payload.Code, payload.Message);
                        }
                    }

                }

                 _contextFactory.Populate(Context, new AuthRawData()
                 {
                     UserEmail = userProfile.PersonalEmail,
                     AccessToken = token,
                     HeaderValues = values,
                     UserId = userProfile.UserId
                 });
              
                return (AuthStatus.Success, Context, new ClaimsPrincipal(new ClaimsIdentity(new EnrichIdentity()
                {              
                    PersonalEmail = userProfile.PersonalEmail,
                    UserId = userProfile.UserId
                }, new[] { new Claim("scope", "admin") })));
            }
            catch (UnauthorizedAccessException)
            {
                return (AuthStatus.Unauthorized, Context, null);
            }

        }
    }
}

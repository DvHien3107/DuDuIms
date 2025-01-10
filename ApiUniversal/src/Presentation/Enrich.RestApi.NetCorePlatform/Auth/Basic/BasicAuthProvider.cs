using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Dto;
using Enrich.IMS.Dto.Authentication;
using Enrich.IMS.Infrastructure.Data.Interface;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;


namespace Enrich.RestApi.NetCorePlatform.Auth.Basic
{
    public sealed class BasicAuthProvider : IAuthProvider
    {
        private readonly IEnrichContextFactory _contextFactory;
        private readonly IMemberRepository _memberRepository;
        private readonly IAccessKeyRepository _accessRepository;

        private readonly string _key;
        public string Name => AuthMode.Basic.ToString();
        public EnrichContext Context { get; }

        public BasicAuthProvider(IEnrichContextFactory contextFactory, EnrichContext context, IConfiguration appConfig, IMemberRepository memberRepository, IAccessKeyRepository accessKeyRepository)
        {
            Context = context;
            _contextFactory = contextFactory;
            _key = appConfig["AuthSettings:Basic:Key"];
            _memberRepository = memberRepository;
            _accessRepository = accessKeyRepository;
        }

        public async Task<(AuthStatus Status, EnrichContext Context, ClaimsPrincipal User)> ValidateAsync(params (string Key, string Value)[] values)
        {
            try
            {
                var token = values.FirstOrDefault(a => a.Key == "Parameter").Value ?? string.Empty;

                if (string.IsNullOrWhiteSpace(token))
                {
                    return (AuthStatus.Unauthorized, Context, null);
                }

                var secretKey = CommonHelper.GetAuthBasicValue(token, _key);
                var member = await _memberRepository.GetMemberBySecretKey(secretKey);

                if (member != null)
                {
                    var userProfile = new UserProfile();
                    _contextFactory.Populate(Context, new AuthRawData()
                    {
                        UserEmail = member.PersonalEmail,
                        FullName = member.FullName,
                        AccessToken = secretKey,
                        HeaderValues = values,
                        UserId = member.Id
                    });

                    return (AuthStatus.Success, Context, new ClaimsPrincipal(new ClaimsIdentity(new EnrichIdentity()
                    {
                        PersonalEmail = userProfile.PersonalEmail,
                        UserId = userProfile.UserId
                    }, new[] { new Claim("scope", "admin") })));
                }
                else if (member == null)
                {
                    var accessKey = await _accessRepository.GetAccessKeyByKey(secretKey);
                    if (accessKey != null && accessKey.Id > 0)
                    {
                        var userProfile = new UserProfile();
                        _contextFactory.Populate(Context, accessKey);

                        return (AuthStatus.Success, Context, new ClaimsPrincipal(new ClaimsIdentity(new EnrichIdentity()
                        {
                            PersonalEmail = accessKey.Name,
                            UserId = accessKey.Id
                        }, new[] { new Claim("scope", "admin") })));
                    }
                    return (AuthStatus.Unauthorized, Context, null);
                }
                return (AuthStatus.Unauthorized, Context, null);
            }
            catch (UnauthorizedAccessException)
            {
                return (AuthStatus.Unauthorized, Context, null);
            }

        }
    }
}

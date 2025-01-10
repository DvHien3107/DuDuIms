using Enrich.Common;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Authentication;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform
{
    public interface IEnrichContextFactory
    {
        void Populate(EnrichContext context, AuthRawData member);

        void Populate(EnrichContext context, AccessKey accessKey);
    }

    public class DefaultEnrichContextFactory : IEnrichContextFactory
    {
        private readonly IMemberRepository _memberRepository;
        private readonly ISystemConfigurationRepository _repositorySystemConfiguration;
        public DefaultEnrichContextFactory(IMemberRepository memberRepository, ISystemConfigurationRepository repositorySystemConfiguration)
        {
            _memberRepository = memberRepository;
            _repositorySystemConfiguration = repositorySystemConfiguration;
        }


        public void Populate(EnrichContext context, AuthRawData rawData)
        {
            ProcessPopulateData(context, rawData);
        }
        public void Populate(EnrichContext context, AccessKey accessKey)
        {
            ProcessPopulateData(context, accessKey);
        }



        private void ProcessPopulateData(EnrichContext context, AuthRawData rawData)
        {
            //change user name for all auto service of IMS system
            if (rawData.UserName == Constants.SystemName)
            {
                context.UserFullName = rawData.FullName;
            }
            else //for user member
            {
                if (rawData.UserId <= 0)
                {
                    throw new Exception("UserId is empty");
                }
                var member = _memberRepository.FindById(rawData.UserId) ?? new Member();

                if (member == null)
                {
                    throw new Exception("Member not found");
                }

                context.Auth.AccessToken = rawData.AccessToken;
                context.UserId = rawData.UserId;
                context.SiteId = member.SiteId;
                context.UserEmail = rawData.UserEmail;// personal email
                context.UserFullName = member.FullName;
                context.UserNumber = member.MemberNumber;
                context.RecurringTime = _repositorySystemConfiguration.GetConfigurationRecuringTime();
                context.Auth.AccessToken = rawData.AccessToken;
                context.SessionId = rawData.SessionId;
            }
        }

        private void ProcessPopulateData(EnrichContext context, AccessKey accessKey)
        {
            context.Auth.AccessToken = accessKey.Key;
            context.SiteId = 1;
            context.UserEmail = accessKey.Name;// personal email
            context.UserFullName = accessKey.Name;
            context.RecurringTime = _repositorySystemConfiguration.GetConfigurationRecuringTime();
        }
    }

    public class AuthRawData
    {

        public long UserId { get; set; }

        public string UserEmail { get; set; }

        public string UserName { get; set; }
        public string FullName { get; set; }

        public string AuthDomain { get; set; }

        public string AccessToken { get; set; }

        public string SessionId { get; set; }

        public (string Key, string Value)[] HeaderValues
        {
            set
            {
                if (value?.Length > 0)
                {
                    AuthDomain = value.FirstOrDefault(a => a.Key == "AuthDomain").Value;
                    SessionId = value.FirstOrDefault(a => a.Key == "X-App-SessionId").Value;
                }
            }
        }
    }
}

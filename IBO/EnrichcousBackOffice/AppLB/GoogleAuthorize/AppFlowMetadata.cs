using EnrichcousBackOffice.AppLB.OptionConfig;
using EnrichcousBackOffice.Utils.OptionConfig;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Calendar.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.AppLB.GoogleAuthorize
{
    public class AppFlowMetadata : FlowMetadata
    {
        private IAuthorizationCodeFlow flow { get; set; }
        private string _UserId { get; set; }
        public override IAuthorizationCodeFlow Flow
        {
            get { return flow; }
        }

        public AppFlowMetadata(string email)
        {
            var _optionConfigurationService = new OptionConfigService();
            var config = _optionConfigurationService.LoadSetting<Config>();
            var flowInitializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = config.Google_ClientID,
                    ClientSecret = config.Google_ClientSecret,
                },
                Scopes = new[] { CalendarService.Scope.Calendar },
                DataStore = new DataStore(),
                Prompt= "consent",
                
            };
            flow = new GoogleAuthorizationCodeFlow(flowInitializer);
            _UserId = email;
        }


        public override string GetUserId(Controller controller)
        {
            if (string.IsNullOrEmpty(_UserId))
            {
                _UserId = controller.Session["EmailAuthorize"].ToString();
            }
            return _UserId;
        }

        public override string AuthCallback => "/AuthCallback/IndexAsync";
    }
}
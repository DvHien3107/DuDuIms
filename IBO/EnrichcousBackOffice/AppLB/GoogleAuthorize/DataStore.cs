using Enrich.Core.Infrastructure;
using Enrich.IServices.Utils.Mailing;
using EnrichcousBackOffice.Controllers;
using EnrichcousBackOffice.Models;
using Google.Apis.Json;
using Google.Apis.Util.Store;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EnrichcousBackOffice.AppLB.GoogleAuthorize
{
    public class DataStore : IDataStore
	{
		private readonly WebDataModel db = new WebDataModel();

        public Task ClearAsync()
		{
			var listmember = db.P_Member;
			listmember.ForEach(c => c.GoogleAuth = null);
			db.SaveChanges();
			return Task.Delay(0);
		}

		public Task DeleteAsync<T>(string key)
		{
			var email = key.Replace("oauth_", "");
			var member = db.P_Member.FirstOrDefault(x => x.PersonalEmail == email);
			member.GoogleAuth = null;
			db.SaveChanges();
			return Task.Delay(0);
		}

		public Task<T> GetAsync<T>(string key)
		{
			var email = key.Replace("oauth_", "");
			var member = db.P_Member.FirstOrDefault(x => x.PersonalEmail == email);
			try
            {
				TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
				if (member.IsAuthorizedGoogle == true)
				{
					// we have it we use that.
					var value = member.GoogleAuth == null ? default(T) : NewtonsoftJsonSerializer.Instance.Deserialize<T>(member.GoogleAuth);
					tcs.SetResult(value);
				}
				else
				{
					tcs.SetResult(default(T));
				}
				return tcs.Task;
			}
            catch 
            {
				member.GoogleAuth = null;
				member.IsAuthorizedGoogle = null;
				var _mailingService = EngineContext.Current.Resolve<IMailingService>();
				var sendemail= _mailingService.SendEmailRequireGoogleAuth(member.PersonalEmail);
				db.SaveChanges();
				throw new ArgumentException("This account's google authorization has expired");
			}
		
		}

		public Task StoreAsync<T>(string key, T value)
		{
			var email = key.Replace("oauth_", "");
			var member = db.P_Member.FirstOrDefault(x => x.PersonalEmail == email);
			var jsonData = JsonConvert.SerializeObject(value);
			member.GoogleAuth = jsonData;
			db.SaveChanges();
			return Task.Delay(0);
		}
	}
}
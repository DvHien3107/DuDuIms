using EnrichcousBackOffice.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Helpers;

namespace EnrichcousBackOffice.Services
{
    public partial class WriteLogErrorService
    {
    
        public void InsertLogError(Exception ex,string SalonName = "")
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (WebDataModel db = new WebDataModel())
                    {
                        IMSLog log = new IMSLog()
                        {
                            Id = Guid.NewGuid().ToString(),
                            CreateBy = "IMS System",
                            CreateOn = DateTime.UtcNow,
                            Url = System.Web.HttpContext.Current.Request.Url.PathAndQuery,
                            RequestUrl = System.Web.HttpContext.Current.Request.Url.PathAndQuery,
                            StatusCode = 500,
                            Success = false,
                            SalonName= SalonName,
                            RequestMethod = "Get",
                            Description = "System Error",
                            JsonRespone = JsonConvert.SerializeObject(ex)
                        };
                        db.IMSLogs.Add(log);
                        db.SaveChanges();
                    }
                    scope.Complete();
                }
            }
            catch
            {

            }
        }
        public void InsertLogError(IMSLog log)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (WebDataModel db = new WebDataModel())
                    {
                        db.IMSLogs.Add(log);
                        db.SaveChanges();
                    }
                    scope.Complete();
                }
            }
            catch
            {

            }
        }
    }
}
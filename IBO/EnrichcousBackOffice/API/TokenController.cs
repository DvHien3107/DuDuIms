using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Configuration;
using System.Net;
using Enrich.Core.Ultils;

namespace EnrichcousBackOffice.API
{
    /// <summary>
    /// Get Token
    /// </summary>
    public class TokenController:ApiController
    {
        /// <summary>
        /// get token
        /// </summary>
        /// <param name="code">(email|pass) encrypt</param>
        /// <returns></returns>
        public HttpResponseMessage Get(string code)
        {
            try
            {
                string _token = SecurityLibrary.Encrypt(code);
                HttpResponseMessage message = new HttpResponseMessage();
                message = Request.CreateResponse(HttpStatusCode.OK, "Authorized");
                //message.Content = new StringContent("Authorized");
                message.Headers.Add("Token", _token );
                message.Headers.Add("TokenExpires", Convert.ToString(60));
                return message;
                
            }
            catch (Exception e)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.Unauthorized, e.Message);
            }
            
        }



    }
}
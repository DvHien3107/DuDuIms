using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.ViewModel;
using Newtonsoft.Json;
using EnrichcousBackOffice.Models.Respon;
using static EnrichcousBackOffice.Services.FeatureService;
using EnrichcousBackOffice.Extensions;
using EnrichcousBackOffice.NextGen;

namespace EnrichcousBackOffice.AppLB
{
    public class ClientRestAPI
    {

        public static HttpResponseMessage CallRestApi(string baseUri, string urlParameters, string token, string method, object valueObject, bool bearer = false, string apiDeter = "Mango POS", string SalonName = "")
        {
            HttpResponseMessage result;
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(baseUri);
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                // List data response.
                HttpResponseMessage response = new HttpResponseMessage();
                if (!string.IsNullOrEmpty(token))
                {
                    if (bearer)
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "Basic " + token);
                    }

                }
                if (method.Equals("get", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    response = client.GetAsync(urlParameters).Result;
                }
                else if (method.Equals("post", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    response = client.PostAsJsonAsync(urlParameters, valueObject).Result;
                }

                result = response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                result = null;
            }

            try
            {
                object resultResponse;
                int statusCode;
                try
                {
                    try { resultResponse = result?.Content.ReadAsAsync<Dictionary<string, object>>().Result ?? new Dictionary<string, object>(); } catch
                    {
                        //var responseTemp = result?.Content.ReadAsAsync<List<Dictionary<string, object>>>().Result;
                        //resultResponse = responseTemp?.First() ?? new Dictionary<string, object>();

                        string responseJson = result.Content.ReadAsStringAsync().Result;
                        responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                        resultResponse = JsonConvert.DeserializeObject<ResponeApiMangoPosFeature>(responseJson);
                    }
                    var dataResult = JsonConvert.DeserializeObject<ApiPOSResponse>(JsonConvert.SerializeObject(resultResponse));
                    statusCode = result?.IsSuccessStatusCode ?? false ? dataResult.StatusCode<int>() : (((int?)result?.StatusCode) ?? 400);
                }
                catch
                {
                    resultResponse = result?.Content.ReadAsStringAsync().Result;
                    statusCode = 500;
                }
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (WebDataModel db = new WebDataModel())
                    {
                        db.IMSLogs.Add(new IMSLog
                        {
                            Id = Guid.NewGuid().ToString(),
                            CreateBy = Authority.GetCurrentMember()?.FullName ?? "IMS System",
                            CreateOn = DateTime.UtcNow,
                            Url = HttpContext.Current?.Request.RawUrl,
                            RequestUrl = baseUri,
                            RequestMethod = method.ToUpper(),
                            StatusCode = statusCode,
                            Success = statusCode < 300,
                            JsonRequest = JsonConvert.SerializeObject(valueObject),
                            JsonRespone = JsonConvert.SerializeObject(resultResponse),
                            Description = apiDeter,
                            SalonName = SalonName,
                        });
                        db.SaveChanges();
                    }
                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                result?.Content.ReadAsStreamAsync().Result.Seek(0, SeekOrigin.Begin);
            }
            return result;
        }
        public class NextGen
        {
            public static async Task<httpResponse> GetAsync(string urlPath, string urlParameters)
            {
                httpResponse httpResponse = new httpResponse();
                try
                {
                    HttpClient client = new HttpClient();
                    // Gọi phương thức bất đồng bộ GetAsync
                    Task<string> responseTask = client.GetStringAsync(DomainConfig.NextGenApi + urlPath + urlParameters);
                    // Đoạn mã dưới đây tiếp tục thực thi trong khi đang chờ đợi phản hồi từ API
                    Console.WriteLine("Đang chờ phản hồi từ API...");
                    //// Đoạn mã này tiếp tục thực thi trong khi đang chờ đợi phản hồi từ API
                    //// Điều này giúp ứng dụng tiếp tục làm việc thay vì chờ đợi
                    //DoSomethingElse();
                    // Chờ phản hồi từ API
                    string response = await responseTask;
                    // Xử lý phản hồi từ API sau khi nhận được
                    Console.WriteLine("Phản hồi từ API: " + response);
                    httpResponse.StatusCode = 200;
                    httpResponse.Message = response;
                }
                catch (Exception e)
                {
                    httpResponse.StatusCode = 500;
                    httpResponse.Message = e.Message;
                    LogModel.WriteLog("--NextGen.GetAsync--" + e.ToString());
                }
               

                return httpResponse;


            }
        }
    }
}

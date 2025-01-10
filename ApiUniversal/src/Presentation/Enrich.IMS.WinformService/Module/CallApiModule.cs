using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RunAtTime.Module
{
    public static class CallApiModule
    {
        public static async Task CallApiRecurring(string url)
        {
            // Replace 'YOUR_API_URL' with the actual API endpoint URL
            System.Net.ServicePointManager.Expect100Continue = false;
            string apiUrl = url;
            using (HttpClient client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5");
                //client.DefaultRequestHeaders.Add("Accept-Language", "EN");

                try
                {
                    LogModule.AddLogRequest("--CallApiRecurring--" + DateTime.UtcNow + $"---{apiUrl}");
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        responseBody = responseBody.Replace("\\r\\n", Environment.NewLine);
                        LogModule.AddLogRespon("--CallApiRecurring--" + DateTime.UtcNow + Environment.NewLine + responseBody);
                    }
                    else
                    {
                        LogModule.AddLogRespon("--CallApiRecurring--" + DateTime.UtcNow + Environment.NewLine + $"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    LogModule.AddLogRespon("--CallApiRecurring--" + DateTime.UtcNow + Environment.NewLine + ex.ToString());
                }
            }
        }

        public static async Task CallApiSyncTwilio()
        {
            try
            {
                // Replace 'YOUR_API_URL' with the actual API endpoint URL
                System.Net.ServicePointManager.Expect100Continue = false;
                string apiUrl = SettingModule.DomainNextGenApi + "/v1/RCPStore/SyncTollFree";
                await doCallApiSyncTwilio(apiUrl);
                apiUrl = SettingModule.DomainNextGenApi + "/v1/RCPStore/RegisTollFree";
                await doCallApiSyncTwilio(apiUrl);
                apiUrl = SettingModule.DomainNextGenApi + "/v1/RCPStore/SyncTollPhone";
                await doCallApiSyncTwilio(apiUrl);
                apiUrl = SettingModule.DomainNextGenApi + "/v1/RCPStore/RegisTollPhone";
                await doCallApiSyncTwilio(apiUrl);

            }
            catch (Exception ex)
            {
                LogModule.AddLogRespon("--ErrorCallApiRecurring--" + DateTime.UtcNow + Environment.NewLine + ex.ToString());
            }
        }

        public static async Task syncMessageTwilio()
        {
            try
            {
                // Replace 'YOUR_API_URL' with the actual API endpoint URL
                System.Net.ServicePointManager.Expect100Continue = false;
                string apiUrl = SettingModule.DomainNextGenApi + "/v1/Twilio/getLstTollFreeToSync";
                await doCallApiGetListAccountTwilio(apiUrl);
                


            }
            catch (Exception ex)
            {
                LogModule.AddLogRespon("--ErrorSyncMessageTwilio--" + DateTime.UtcNow + Environment.NewLine + ex.ToString());
            }
        }

        private static async Task doCallApiSyncTwilio(string apiUrl)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var collection = new List<KeyValuePair<string, string>>();
            //collection.Add(new("FriendlyName", friendlyname));
            var content = new FormUrlEncodedContent(collection);
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            responseContent = responseContent.Replace("\\r\\n", Environment.NewLine);
            LogModule.AddLogRespon($"--CallApiRecurring-{apiUrl}--" + DateTime.UtcNow + Environment.NewLine + responseContent);
        }

        private static async Task doCallApiGetListAccountTwilio(string apiUrl)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
                var resultModel = JsonConvert.DeserializeObject<dynamic>(result);
                var test = resultModel.obj_data;
                foreach (var item in test)
                {
                    string cmdComt = $"\"\"C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe\" --new-window \"{SettingModule.DomainNextGenApi}/html/syncTwilio.html?SId={item.sId}&AuthToken={item.authToken}&maxRVCNo={item.maxRVCNo}&countRVCNo={item.countRVCNo}&dataFrom={item.dataFrom}\" --user-data-dir=\"C://chrome-dev-disabled-security\" --disable-web-security --disable-site-isolation-trials\"";
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        Arguments = "/c " + cmdComt,
                        CreateNoWindow = true,
                        WorkingDirectory = string.Empty,
                    };
                    process.Start();
                    process.WaitForExit();
                    Console.WriteLine(process.StandardOutput.ReadToEnd());
                    Console.WriteLine(process.StandardError.ReadToEnd());

                }
                LogModule.AddLogRespon($"--CallApiRecurring-{apiUrl}--" + DateTime.UtcNow + Environment.NewLine + result);
            }
            catch (Exception ex ) 
            {
                LogModule.AddLogRespon($"----Error- oCallApiGetListAccountTwilio-{apiUrl}--" + DateTime.UtcNow + Environment.NewLine + ex.ToString());
            }


        }
    }
}

using Enrich.DataTransfer;
using Enrich.IServices;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using Serilog.Sinks.Graylog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.TwiML.Messaging;

namespace Enrich.Services
{
    public class LogService : ILogService
    {
        private ILogger _logger = Serilog.Log.Logger;
        private readonly EnrichContext _context;

        private readonly List<IDisposable> _disposableProperties = new List<IDisposable>();
        public LogService(EnrichContext context)
        {
            _context = context;
        }
        public void WriteLog(string Message, string Type)
        {
            string path = "C:\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = "C:\\Logs\\"+ Type+ "_" + DateTime.UtcNow.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
                }
            }
        }

        public void Debug(string message)
        {
            WriteLog(message, "Debug");
        }

        public void Error(string message)
        {
            WriteLog(message, "Error");
        }

        public void Error(string message, object extendData = null, bool isExternal = false)
        {
            PushExtendInfos(ref message, extendData, isExternal);
            _logger.Error(message);
        }

        public void Error(Exception ex, string message)
        {
            WriteLog(ex.ToString(), "Error");
        }

        /// <summary>
        /// Log with level = 2
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            PushExtendInfos(ref message, false);
            _logger.Information(message);
            WriteLog(message, "Info");
        }

        /// <summary>
        /// Log with level = 2
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <param name="extendData"> it should be a class or a dictionary</param>
        public void Info(string message, object extendData = null, bool isExternal = false)
        {
            PushExtendInfos(ref message, extendData, isExternal);
            _logger.Information(message);
        }

        public void Warning(string message)
        {
            WriteLog(message, "Warning");
        }

        public void Warning(string message, object extendData = null, bool isExternal = false)
        {
            PushExtendInfos(ref message, extendData, isExternal);
            _logger.Warning(message);
        }

        public void Info(Exception ex, string message)
        {
            WriteLog(ex.ToString(), "Info");
        }

        public void GrayLogForDebug(Action<ILogService> action)
        {
            //#if DEBUG
            //            action(this);
            //#else
            //                        using (LogContext.PushProperty(CustomGelfConverter.PropertyName_UseDebug, true))
            //                        {
            //                            action(this);
            //                        }
            //#endif
        }

        public void Release()
        {
            foreach (var item in _disposableProperties)
            {
                try
                {
                    item.Dispose();
                }
                catch { }
            }

            _disposableProperties.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isExternal">use for api, so we dont add default data</param>
        private void PushExtendInfos(ref string message, bool isExternal = false)
        {
            if (_context == null)
            {
                return;
            }

            System.Web.HttpContext context = System.Web.HttpContext.Current;

            string ipAddress = context?.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                _disposableProperties.Add(LogContext.PushProperty("ipAddress", addresses[0]));
            }
            else
            {
                _disposableProperties.Add(LogContext.PushProperty("ipAddress", context?.Request.ServerVariables["REMOTE_ADDR"]));
            }

            if (!string.IsNullOrEmpty(_context.ApplicationName))
            {
                _disposableProperties.Add(LogContext.PushProperty("ApplicationName", _context.ApplicationName));
            }

            if (!isExternal)
            {
                if (!string.IsNullOrWhiteSpace(_context.MemberEmail))
                {
                    var authInfo = $"{_context?.MemberFullName}";
                    message = $"[{authInfo}] {message}";

                    _disposableProperties.Add(LogContext.PushProperty("User", _context?.MemberEmail));
                }
                _disposableProperties.Add(LogContext.PushProperty("TraceTrackId", _context?.TrackTraceId));

                message = $"{_context?.Id} {message}";
                return;
            }
            else
            {
                LogContext.Reset();
                message = $"[{_context?.MemberFullName}] {message}";
            }
        }

        private void PushExtendInfos(ref string message, object extendData = null, bool isExternal = false)
        {
            PushExtendInfos(ref message, isExternal);

            if (extendData != null)
            {
                var jsonData = JsonConvert.SerializeObject(extendData);
                var dictionaryData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData).Where(x => x.Value != null);
                if (dictionaryData != null)
                {
                    foreach (var item in dictionaryData)
                    {
                        if (item.Value is null)
                            continue;
                        else if (item.Value is string)
                            LogContext.PushProperty(item.Key, item.Value);
                        else
                            LogContext.PushProperty(item.Key, JsonConvert.SerializeObject(item.Value));

                    }
                }
            }
        }
    }
}

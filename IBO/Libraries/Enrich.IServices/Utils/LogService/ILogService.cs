using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices
{
    public interface ILogService
    {
        void Debug(string message);

        void Warning(string message);

        /// <summary>
        /// Log with level = 3
        /// </summary>
        /// <param name="message"></param>
        /// <param name="extendData"></param>
        /// <param name="isExternal">use for api, we dont add default data</param>
        void Warning(string message, object extendData = null, bool isExternal = false);

        void Error(string message);

        void Error(Exception ex, string message);

        /// <summary>
        /// Log with level = 4
        /// </summary>
        /// <param name="message"></param>
        /// <param name="extendData"></param>
        /// <param name="isExternal">use for api, we dont add default data</param>
        void Error(string message, object extendData = null, bool isExternal = false);

        /// <summary>
        /// Log with level = 2
        /// </summary>    
        /// <param name="message"></param>    
        void Info(string message);

        /// <summary>
        /// Log with level = 2
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <param name="extendData"> it should be a class or a dictionary</param> 
        /// /// <param name="isExternal">use for api, we dont add default data</param>
        void Info(string message, object extendData = null, bool isExternal = false);

        void Info(Exception ex, string message);

        void GrayLogForDebug(Action<ILogService> action);
    }
}

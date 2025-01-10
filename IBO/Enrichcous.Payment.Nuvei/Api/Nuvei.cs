using System.Linq;
using Enrichcous.Payment.Nuvei.Config;
using Enrichcous.Payment.Nuvei.Config.Enums;
using Enrichcous.Payment.Nuvei.Config.Models;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using NuveiClient;

namespace Enrichcous.Payment.Nuvei.Api
{
    public abstract class Nuvei
    {
        protected bool AccountTest = Constant.AccountTest ?? true;
        public Terminal GetTerminal(string currency)
        {
            return Constant.Terminals.FirstOrDefault(terminal => terminal.Currency == "MCP") ?? Constant.Terminals.FirstOrDefault(terminal => terminal.Currency == currency);
        }
        public Terminal GetTerminalById(string id)
        {
            return Constant.Terminals.FirstOrDefault(terminal => terminal.Id == id);
        }

        public void HandleError(Response _response, string _responseHash = null)
        {
            ResponseHandleError response = JsonConvert.DeserializeObject<ResponseHandleError>(JsonConvert.SerializeObject(_response));
            if (response.IsError)
            {
                throw new AppHandleException().Code(response.ErrorCode).ExMessage(response.ErrorString);
            }
            else if (string.IsNullOrEmpty(_responseHash) == false && response.Hash != _responseHash)
            {
                // throw new AppHandleException().ExMessage(Func.Error(IMS_MSG.E_NUVEI_SECURITY));
            }
        }
    }

    public class ResponseHandleError
    {
        public bool IsError;
        public string ErrorString;
        public string ErrorCode;
        public string Hash;
    }
}
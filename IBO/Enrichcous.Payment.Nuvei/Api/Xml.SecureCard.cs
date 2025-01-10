using System;
using Enrichcous.Payment.Nuvei.Config.Enums;
using Enrichcous.Payment.Nuvei.Config.Models;
using Enrichcous.Payment.Nuvei.Models;
using Enrichcous.Payment.Nuvei.Utils;
using Inner.Libs.Helpful;
using NuveiClient;

namespace Enrichcous.Payment.Nuvei.Api
{
    public class XmlSecureCard : Nuvei
    {
        private readonly CardUtil cardUtil = new CardUtil();
        
        /// <summary>
        /// Resist card
        /// https://helpdesk.nuvei.com/doku.php?id=developer:api_specification:xml_secure_card_features#registration
        /// </summary>
        /// <param name="card"></param>
        public void Registration(ref CardInfo card)
        {
            // Card info
            cardUtil.RemakeCardInfo(card);
            Terminal terminal = GetTerminal(card.Currency);

            // Make xml request
            XmlSecureCardRegRequest secureReg = new XmlSecureCardRegRequest(card.MerchantCardReference, terminal.Id, card.CardNumber, card.CardExpiry, card.CardType, card.CardHolderName);
          
            secureReg.SetDontCheckSecurity("Y");//Y: neu bo viec check CVV
            if (!String.IsNullOrEmpty(card.CardCSC))
            {
                secureReg.SetCvv(card.CardCSC);
            }
            // Make xml response
            XmlSecureCardRegResponse response = secureReg.ProcessRequest(terminal.Secret, AccountTest, terminal.Gateway);

           // System.Diagnostics.Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));

          

            String responseHash = Response.GetResponseHash(terminal.Id + response.MerchantRef + response.CardRef + response.DateTimeHashString + terminal.Secret);

         

            HandleError(response, responseHash); 
            card.CardReference = response.CardRef;
            card.MerchantReference = response.MerchantRef;
            card.TerminalId = terminal.Id;
            if (string.IsNullOrEmpty(response.CardRef))
            {
                throw new AppHandleException().ExMessage(Func.Error(IMS_MSG.E_SYS_DEFAULT));
            }
        }

        /// <summary>
        /// Update
        /// https://helpdesk.nuvei.com/doku.php?id=developer:api_specification:xml_secure_card_features#update
        /// </summary>
        /// <param name="card"></param>
        public void Update(ref CardInfo card)
        {
            // Card info
            cardUtil.RemakeCardInfo(card);
            Terminal terminal = GetTerminalById(card.TerminalId);
            // Make xml request
            XmlSecureCardUpdRequest secureUpd = new XmlSecureCardUpdRequest(card.MerchantReference, terminal.Id, card.CardNumber, card.CardExpiry, card.CardType, card.CardHolderName);
            secureUpd.SetDontCheckSecurity("Y");
            if (!String.IsNullOrEmpty(card.CardCSC))
            {
                secureUpd.SetCvv(card.CardCSC);
            }
            // Make xml response
            XmlSecureCardUpdResponse response = secureUpd.ProcessRequest(terminal.Secret, AccountTest, terminal.Gateway);
            String responseHash = Response.GetResponseHash (terminal.Id + response.MerchantRef + response.CardRef + response.DateTimeHashString + terminal.Secret);
            HandleError(response, responseHash);
        }

        /// <summary>
        /// Delete
        /// https://helpdesk.nuvei.com/doku.php?id=developer:api_specification:xml_secure_card_features#removal
        /// </summary>
        /// <param name="card"></param>
        public void Delete(CardInfo card)
        {
            Terminal terminal = GetTerminalById(card.TerminalId);
            XmlSecureCardDelRequest secureDel = new XmlSecureCardDelRequest (card.MerchantReference, terminal.Id, card.CardReference);
            XmlSecureCardDelResponse response = secureDel.ProcessRequest(terminal.Secret, AccountTest, terminal.Gateway);
            String responseHash = Response.GetResponseHash (terminal.Id + response.MerchantRef + response.DateTimeHashString + terminal.Secret);
            HandleError(response, responseHash);
        }
    }
}
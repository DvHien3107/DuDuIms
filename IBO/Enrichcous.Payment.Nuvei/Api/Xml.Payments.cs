using System;
using System.Globalization;
using Enrichcous.Payment.Nuvei.Config.Models;
using Enrichcous.Payment.Nuvei.Models;
using Newtonsoft.Json;
using NuveiClient;

namespace Enrichcous.Payment.Nuvei.Api
{
    public class XmlPayments: Nuvei
    {
        /// <summary>
        /// https://helpdesk.nuvei.com/doku.php?id=developer:api_specification:xml_payment_features#payment
        /// </summary>
        /// <param name="card"></param>
        /// <param name="payment"></param>
        public void Authorisation(CardInfo card, ref PaymentInfo payment)
        {
            // Terminal
            Terminal terminal = GetTerminalById(card.TerminalId);
            XmlAuthRequest request = new XmlAuthRequest (terminal.Id, payment.OrderCode, card.Currency, payment.Amount, card.CardType);
            request.SetCardNumber (card.CardReference);
            request.SetEmail(card.CardEmail);
            request.SetAvs (card.CardAddressStreet, "", card.CardZipCode);
            request.SetCountry (card.CardCountry);
            request.SetCity (card.CardCity);
            request.SetPhone (card.CardPhone);
            request.SetMotoTrans ();
            if (terminal.IsMultiCur()) {
                request.SetMultiCur ();
            }

        //    System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(request));

            XmlAuthResponse response = request.ProcessRequest (terminal.Secret, AccountTest, terminal.Gateway);

        //    System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(response));

            String responseHash = Response.GetResponseHash (
                terminal.Id + response.UniqueRef + (terminal.IsMultiCur() ? "" : card.Currency) + payment.Amount.ToString (CultureInfo.InvariantCulture) +
                response.DateTimeHashString + response.ResponseCode + response.ResponseText + terminal.Secret
            );

            System.Diagnostics.Debug.WriteLine(responseHash);

            HandleError(response, responseHash);
            payment.ResponseCode = response.ResponseCode;
            payment.ResponseText = response.ResponseText;
            payment.ApprovalCode = response.ApprovalCode;
            payment.AvsResponse = response.AvsResponse;
            payment.CvvResponse = response.CvvResponse;
            payment.UniqueRef = response.UniqueRef;
        }
    }
}
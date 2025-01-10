using System;
using System.Linq;
using System.Text.RegularExpressions;
using Enrichcous.Payment.Mxmerchant.Config.Enums;
using Enrichcous.Payment.Mxmerchant.Models;
using Inner.Libs.Helpful;

namespace Enrichcous.Payment.Mxmerchant.Utils
{
    public class CardUtil
    {
        /// <summary>
        /// Get card type by card number
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public ECardType CardType(string cardNumber)
        {
            if (new Regex(@"^4[0-9]{6,}$").IsMatch(cardNumber)) return ECardType.VisaCredit;
            if (new Regex(@"^3[47][0-9]{5,}$").IsMatch(cardNumber)) return ECardType.AmericanExpress;
            if (new Regex(@"^5[1-5][0-9]{5,}|222[1-9][0-9]{3,}|22[3-9][0-9]{4,}|2[3-6][0-9]{5,}|27[01][0-9]{4,}|2720[0-9]{3,}$").IsMatch(cardNumber)) return ECardType.MasterCard;
            if (new Regex(@"^(4026|417500|4405|4508|4844|4913|4917)\d+$").IsMatch(cardNumber)) return ECardType.VisaElectron;
            if (new Regex(@"^(5018|5020|5038|6304|6759|6761|6763)[0-9]{8,15}$").IsMatch(cardNumber)) return ECardType.Maestro;
            if (new Regex(@"^(6304|6706|6709|6771)[0-9]{12,15}$").IsMatch(cardNumber)) return ECardType.Laser;
            if (new Regex(@"^3(?:0[0-5]|[68][0-9])[0-9]{11}$").IsMatch(cardNumber)) return ECardType.Diners;
            if (new Regex(@"^65[4-9][0-9]{13}|64[4-9][0-9]{13}|6011[0-9]{12}|(622(?:12[6-9]|1[3-9][0-9]|[2-8][0-9][0-9]|9[01][0-9]|92[0-5])[0-9]{10})$").IsMatch(cardNumber)) return ECardType.Discover;
            if (new Regex(@"^(?:2131|1800|35\d{3})\d{11}$").IsMatch(cardNumber)) return ECardType.JCB;
            if (cardNumber.Where((e) => e >= '0' && e <= '9').Reverse().Select((e, i) => ((int) e - 48) * (i % 2 == 0 ? 1 : 2)).Sum((e) => e / 10 + e % 10) == 0) return ECardType.NotFormatted;
            return ECardType.Unknown;
        }

        /// <summary>
        /// Detect card type by card number
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Invalid card number</exception>
        public ECardType CardTypeDetect(string cardNumber)
        {
            ECardType cardType = CardType(cardNumber);
            Enum msg = null;
            switch (cardType)
            {
                case ECardType.Unknown:
                    msg = IMS_MSG.E_CARD_UNKNOWN;
                    break;
                case ECardType.NotFormatted:
                    msg = IMS_MSG.E_CARD_NOT_FORMATTED;
                    break;
            }

            if (msg != null)
            {
                throw new AppHandleException(msg.Code<string>(), msg.Text());
            }

            return cardType;
        }

        /// <summary>
        /// CardExpiryCheck
        /// </summary>
        /// <param name="expiry"></param>
        /// <returns></returns>
        /// <exception cref="AppHandleException"></exception>

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="cardInfo"></param>
        ///// <returns></returns>
        //public bool RemakeCardInfo(CardInfo cardInfo)
        //{
        //    cardInfo.CardType = CardTypeDetect(cardInfo.CardNumber).Code<string>();
        //    cardInfo.CardExpiry = CardExpiryCheck(cardInfo.CardExpiry);
        //    if (string.IsNullOrEmpty(cardInfo.CardHolderName))
        //    {
        //        cardInfo.CardHolderName = $"{cardInfo.CardFirstName} {cardInfo.CardLastName}";
        //    }
        //    cardInfo.CardHolderName = cardInfo.CardHolderName?.Trim();
        //    return true;
        //}
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Card_Account_Transactions
{
    public static class Test
    {
        public static string Main()
        {
            try
            {
                var OAuth = new newOAuth(ConfigFactory.endPointRequestToken);
                OAuth_Utilities U = new OAuth_Utilities();
                var t = new RetrieveTokens();
                NameValueCollection toke = new NameValueCollection();
                    //toke = t.getAccessToken();
                    toke.Add("oauth_token", "-----------------------------");
                    toke.Add("oauth_token_secret", "-----------------------------");
                PaymentFactory p = new PaymentFactory();
                p.merchantId = "1111111";
                p.tenderType = "Card";
                p.amount = "0.01";
                p.cardAccount = new cardAccount();
                p.cardAccount.expiryMonth = "07";
                p.cardAccount.expiryYear = "2016";
                p.cardAccount.cvv = "123";
                p.cardAccount.avsStreet = "1234";

                OAuth = new newOAuth(ConfigFactory.payment, toke);
                string Headers = OAuth.createHeaders();
                string createdPayment = U.sendRequest(ConfigFactory.payment, Headers, p);

                Console.WriteLine(createdPayment);
                return "";
            }
            catch(Exception e) { return e.ToString(); }

        }

    }
}

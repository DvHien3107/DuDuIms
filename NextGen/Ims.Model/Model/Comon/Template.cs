using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Comon
{
    public class Template
    {
        public static readonly string InvoicePayment = $@"
<html>
<style type=""text/css"">
      .email {{
        max-width: 480px;
        margin: 1rem auto;
        border-radius: 10px;
        border-top: #d74034 2px solid;
        border-bottom: #d74034 2px solid;
        box-shadow: 0 2px 18px rgba(0, 0, 0, 0.2);
        padding: 1.5rem;
        font-family: Arial, Helvetica, sans-serif;
      }}
      .email .email-head {{
        border-bottom: 1px solid rgba(0, 0, 0, 0.2);
        padding-bottom: 1rem;
      }}
      .email .email-head .head-img {{
        max-width: 240px;
        padding: 0 0.5rem;
        display: block;
        margin: 0 auto;
      }}

      .email .email-head .head-img img {{
        width: 100%;
      }}
      .email-body .invoice-icon {{
        max-width: 80px;
        margin: 1rem auto;
      }}
      .email-body .invoice-icon img {{
        width: 100%;
      }}

      .email-body .body-text {{
        padding: 2rem 0 1rem;
        text-align: center;
        font-size: 1.15rem;
      }}
      .email-body .body-text.bottom-text {{
        padding: 2rem 0 1rem;
        text-align: center;
        font-size: 0.8rem;
      }}
      .email-body .body-text .body-greeting {{
        font-weight: bold;
        margin-bottom: 1rem;
      }}

      .email-body .body-table {{
        text-align: left;
      }}
      .email-body .body-table table {{
        width: 100%;
        font-size: 1.1rem;
      }}
      .email-body .body-table table .total {{
        background-color: hsla(4, 67%, 52%, 0.12);
        border-radius: 8px;
        color: #d74034;
      }}
      .email-body .body-table table .item {{
        border-radius: 8px;
        color: #d74034;
      }}
      .email-body .body-table table th,
      .email-body .body-table table td {{
        padding: 10px;
      }}
      .email-body .body-table table tr:first-child th {{
        border-bottom: 1px solid rgba(0, 0, 0, 0.2);
      }}
      .email-body .body-table table tr td:last-child {{
        text-align: right;
      }}
      .email-body .body-table table tr th:last-child {{
        text-align: right;
      }}
      .email-body .body-table table tr:last-child th:first-child {{
        border-radius: 8px 0 0 8px;
      }}
      .email-body .body-table table tr:last-child th:last-child {{
        border-radius: 0 8px 8px 0;
      }}
      .email-footer {{
        border-top: 1px solid rgba(0, 0, 0, 0.2);
      }}
      .email-footer .footer-text {{
        font-size: 0.8rem;
        text-align: center;
        padding-top: 1rem;
      }}
      .email-footer .footer-text a {{
        color: #d74034;
      }}
    </style>
  </head>
  <body>
    <div class=""email"">
      <div class=""email-head"">
        <div class=""head-img"">
          <img
            src=""https://wpfystatic.b-cdn.net/rahul/email.png""
            alt=""damnitrahul-logo""
          />
        </div>
      </div>
      <div class=""email-body"">
        <div class=""body-text"">
          <div class=""body-greeting"">
            Hi, Rahul!
          </div>
          Your order has been successfully completed and delivered to You!
        </div>
        <div class=""invoice-icon"">
          <img src=""https://wpfystatic.b-cdn.net/rahul/billl.png"" alt=""invoice-icon"" />
        </div>
        <div class=""body-table"">
          <table>
            <tr class=""item"">
              <th>Service Provided</th>
              <th>Amount</th>
            </tr>
            <tr>
              <td>Custom Graphic/Illustration</td>
              <td>₹1500</td>
            </tr>
            <tr>
              <td>Custom Graphic/Illustration</td>
              <td>₹1500</td>
            </tr>
            <tr>
              <td>Custom Graphic/Illustration</td>
              <td>₹1500</td>
            </tr>
            <tr class=""total"">
              <th>Total</th>
              <th>₹1500</th>
            </tr>
          </table>
        </div>
        <div class=""body-text bottom-text"">
          Thank You for giving me the opportunity to work on this project. I
          hope the product met your expectations. I look forward to working with
          You &#708;_&#708;
        </div>
      </div>
      <div class=""email-footer"">
        <div class=""footer-text"">
          &copy; <a href=""https://damnitrahul.com/""  target=""_blank"">damnitrahul.com</a>
        </div>
      </div>
    </div>
  </body>

</html>
";

        public static string StorerLoginTemplate = $@"<head>
    <title></title>
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">

    <style type=""text/css"">

    </style>
    <!--<![endif]-->
    <style type=""text/css"">
    </style>
</head>
<body style=""background: #F9F9F9;"">
    <div style=""background-color:#F9F9F9;"">
        <div style=""max-width:640px;margin:0 auto;box-shadow:0px 1px 5px rgba(0,0,0,0.1);border-radius:4px;overflow:hidden"">
            <div style=""margin: 0px auto; max-width: 640px; background: #3c75f0;"">
                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size: 0px; width: 100%; background: #3c75f0;"" align=""center"" border=""0"">
                    <tbody>
                        <tr>
                            <td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:57px;"">
                                <div style=""cursor:auto;color:white;font-family:Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-size:36px;font-weight:600;line-height:36px;text-align:center;"">
                                    Welcome to Swiftpos!
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div style=""margin:0px auto;max-width:640px;background:#ffffff;"">
                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;background:#ffffff;"" align=""center"" border=""0"">
                    <tbody>
                        <tr>
                            <td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:40px 70px;"">
                                <div aria-labelledby=""mj-column-per-100"" class=""mj-column-per-100 outlook-group-fix"" style=""vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;"">
                                    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" border=""0"">
                                        <tbody>
                                            <tr>
                                                <td style=""word-break:break-word;font-size:0px;padding:0px 0px 20px;"" align=""left"">
                                                    <div style=""cursor:auto;color:#737F8D;font-family:Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-size:16px;line-height:24px;text-align:left;"">
                                                       

                                                        <h2 style=""font-family: Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-weight: 500;font-size: 20px;color: #4F545C;letter-spacing: 0.27px;"">
                                                            Hey [CustomerName],
                                                        </h2>
                                                        <p>Wowwee! Thanks for using an POS system with Swiftpos! Before we get continued, we'll need to verify your email.</p>
                                                        <p>
                                                            Please enter the following verification code to access your Swiftpos Account.
                                                        </p>
                                                        <div style=""border:0;box-sizing:inherit;font:inherit;font-size:100%;margin:0;padding:0;vertical-align:baseline"">
                                                            <div style=""border:0;box-sizing:inherit;font:inherit;font-size:100%;height:60px;margin:0;padding:0;vertical-align:middle"" align=""left"">
                                                                <p style=""border:0;box-sizing:inherit;color:rgb(34,34,34);font:inherit;font-size:20px;font-weight:bold;line-height:20px;margin:0;padding:0;vertical-align:baseline"">
                                                                    [VerifyCode]
                                                                </p>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <div style=""margin:0px auto;max-width:640px;background:transparent;"">
            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;background:transparent;"" align=""center"" border=""0"">
                <tbody>
                    <tr>
                        <td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:0px;"">
                            <div aria-labelledby=""mj-column-per-100"" class=""mj-column-per-100 outlook-group-fix"" style=""vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;""><table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" border=""0""><tbody><tr><td style=""word-break:break-word;font-size:0px;""><div style=""font-size:1px;line-height:12px;"">&nbsp;</div></td></tr></tbody></table></div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        <div style=""margin:0px auto;max-width:640px;background:transparent;"">
            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-size:0px;width:100%;background:transparent;"" align=""center"" border=""0"">
                <tbody>
                    <tr>
                        <td style=""text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:20px 0px;"">
                            <div aria-labelledby=""mj-column-per-100"" class=""mj-column-per-100 outlook-group-fix"" style=""vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;"">
                                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" border=""0"">
                                    <tbody>
                                        <tr>
                                            <td style=""word-break:break-word;font-size:0px;padding:0px;"" align=""center"">
                                                <div style=""cursor:auto;color:#99AAB5;font-family:Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-size:12px;line-height:24px;text-align:center;"">
                                                    Sent by Swift Pos • <a href=""https://swiftpos.us/"" style=""color:#1EB0F4;text-decoration:none;"" target=""_blank"">check our blog</a> • <a href=""https://swiftpos.us/#contactUs"" style=""color:#1EB0F4;text-decoration:none;"" target=""_blank"">contact us</a>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style=""word-break:break-word;font-size:0px;padding:0px;"" align=""center"">
                                                <div style=""cursor:auto;color:#99AAB5;font-family:Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-size:12px;line-height:24px;text-align:center;"">
                                                    3076 Fifth Ave, San Diego, CA 92103
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

</body>";

        public static string SupportNotifyTemplate = "<head>\r\n    <title></title>\r\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\r\n    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">\r\n    <style type=\"text/css\"></style>\r\n    <!--\r\n              <![endif]-->\r\n    <style type=\"text/css\"></style>\r\n  </head>\r\n  <body style=\"background: #F9F9F9;\">\r\n    <div style=\"background-color:#F9F9F9;\">\r\n      <div style=\"max-width:640px;margin:0 auto;box-shadow:0px 1px 5px rgba(0,0,0,0.1);border-radius:4px;overflow:hidden\">\r\n        <div style=\"margin: 0px auto; max-width: 640px; background: #3c75f0;\">\r\n          <table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"font-size: 0px; width: 100%; background: #3c75f0;\" align=\"center\" border=\"0\">\r\n            <tbody>\r\n              <tr>\r\n                <td style=\"text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:57px;\">\r\n                  <div style=\"cursor:auto;color:white;font-family:Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-size:36px;font-weight:600;line-height:36px;text-align:center;\"> Notification of Recent Login! </div>\r\n                </td>\r\n              </tr>\r\n            </tbody>\r\n          </table>\r\n        </div>\r\n        <div style=\"margin:0px auto;max-width:640px;background:#ffffff;\">\r\n          <table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"font-size:0px;width:100%;background:#ffffff;\" align=\"center\" border=\"0\">\r\n            <tbody>\r\n              <tr>\r\n                <td style=\"text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:40px 70px;\">\r\n                  <div aria-labelledby=\"mj-column-per-100\" class=\"mj-column-per-100 outlook-group-fix\" style=\"vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;\">\r\n                    <table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n                      <tbody>\r\n                        <tr>\r\n                          <td style=\"word-break:break-word;font-size:0px;padding:0px 0px 20px;\" align=\"left\">\r\n                            <div style=\"cursor:auto;color:#737F8D;font-family:Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-size:16px;line-height:24px;text-align:left;\">\r\n                           \r\n                              <h2 style=\"font-family: Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-weight: 500;font-size: 20px;color: #4F545C;letter-spacing: 0.27px;\"> Dear [UserName], </h2>\r\n                              <p> We wanted to inform you that a support login was detected on your Swiftpos Account. Please review the details below: </p>\r\n                              <div style=\"border:0;box-sizing:inherit;font:inherit;font-size:100%;margin:0;padding:0;vertical-align:baseline\">\r\n                                <div style=\"border:0;box-sizing:inherit;font:inherit;font-size:100%;height:60px;margin:0;padding:0;vertical-align:middle\" align=\"left\">\r\n                                  <p style=\"border:0;box-sizing:inherit;color:rgb(34,34,34);font:inherit;font-weight:bold;line-height:20px;margin:0;padding:0;vertical-align:baseline\"> Login Time: [LoginTime] </p>\r\n                                  <p style=\"border:0;box-sizing:inherit;color:rgb(34,34,34);font:inherit;font-weight:bold;line-height:20px;margin:0;padding:0;vertical-align:baseline\"> Support Member: [SupportMember] <br />\r\n                                  </p>\r\n                                  <p style=\"border:0;box-sizing:inherit;color:rgb(34,34,34);font:inherit;font-weight:bold;line-height:20px;margin:0;padding:0;vertical-align:baseline\"> Reason Login: [SupportReason] <br />\r\n                                  </p>\r\n                                </div>\r\n                              </div>\r\n                              <p> If you recognize this login and have initiated it, you can disregard this email. \r\n                                <br /> \r\n                                However, If you believe your account has been compromised, please contact our support team immediately at +1 (619) 255-7180. \r\n                                \r\n                              <p> Thank you for your attention to this matter. </p>\r\n                            </div>\r\n                          </td>\r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n                  </div>\r\n                </td>\r\n              </tr>\r\n            </tbody>\r\n          </table>\r\n        </div>\r\n      </div>\r\n      <div style=\"margin:0px auto;max-width:640px;background:transparent;\">\r\n        <table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"font-size:0px;width:100%;background:transparent;\" align=\"center\" border=\"0\">\r\n          <tbody>\r\n            <tr>\r\n              <td style=\"text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:0px;\">\r\n                <div aria-labelledby=\"mj-column-per-100\" class=\"mj-column-per-100 outlook-group-fix\" style=\"vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;\">\r\n                  <table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n                    <tbody>\r\n                      <tr>\r\n                        <td style=\"word-break:break-word;font-size:0px;\">\r\n                          <div style=\"font-size:1px;line-height:12px;\">&nbsp;</div>\r\n                        </td>\r\n                      </tr>\r\n                    </tbody>\r\n                  </table>\r\n                </div>\r\n              </td>\r\n            </tr>\r\n          </tbody>\r\n        </table>\r\n      </div>\r\n      <div style=\"margin:0px auto;max-width:640px;background:transparent;\">\r\n        <table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"font-size:0px;width:100%;background:transparent;\" align=\"center\" border=\"0\">\r\n          <tbody>\r\n            <tr>\r\n              <td style=\"text-align:center;vertical-align:top;direction:ltr;font-size:0px;padding:20px 0px;\">\r\n                <div aria-labelledby=\"mj-column-per-100\" class=\"mj-column-per-100 outlook-group-fix\" style=\"vertical-align:top;display:inline-block;direction:ltr;font-size:13px;text-align:left;width:100%;\">\r\n                  <table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n                    <tbody>\r\n                      <tr>\r\n                        <td style=\"word-break:break-word;font-size:0px;padding:0px;\" align=\"center\">\r\n                          <div style=\"cursor:auto;color:#99AAB5;font-family:Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-size:12px;line-height:24px;text-align:center;\"> \r\n                            Sent by Swift Team � <a href=\"https://swiftpos.us/\" style=\"color:#1EB0F4;text-decoration:none;\" target=\"_blank\">\r\n                                check our blog</a> � <a href=\"https://swiftpos.us/#contactUs\" style=\"color:#1EB0F4;text-decoration:none;\" target=\"_blank\">contact us</a>\r\n                          </div>\r\n                        </td>\r\n                      </tr>\r\n                      <tr>\r\n                        <td style=\"word-break:break-word;font-size:0px;padding:0px;\" align=\"center\">\r\n                          <div style=\"cursor:auto;color:#99AAB5;font-family:Whitney, Helvetica Neue, Helvetica, Arial, Lucida Grande, sans-serif;font-size:12px;line-height:24px;text-align:center;\"> \r\n                            3076 Fifth Ave, San Diego, CA 92103</div>\r\n                        </td>\r\n                      </tr>\r\n                    </tbody>\r\n                  </table>\r\n                </div>\r\n              </td>\r\n            </tr>\r\n          </tbody>\r\n        </table>\r\n      </div>\r\n    </div>\r\n  </body>";

    }
}

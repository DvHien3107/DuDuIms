using Enrichcous.Payment.Mxmerchant.Models;
using EnrichcousBackOffice.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.AppLB
{
    public class UserContent
    {

        public const string CUSTOMIZE_FORM_SESSION = "CustomizeForm_"; //+membernumber
        public const string CUSTOMIZE_FORM_SESSION_TITLE = "CustomizeFormTitle_";//+membernumber
        public const string SESSION_FRIENDLY_MEMBER = "SESSION_FRIENDLY_MEMBER";


        public enum EMAIL_TEMPLATE_TAB
        {
            MERCHANT_OWNER_NAME,
            MERCHANT_BUSINESS_NAME,
            COMPANY_NAME,
            COMPANY_EMAIL,
            COMPANY_ADDRESS,
            YOUR_NAME,
            YOUR_PHONE,
            YOUR_EMAIL
        };


        //public enum ORDER_STATUS
        //{
        //    Draft,
        //    Lead,
        //    Lost,
        //    Closed,
        //    Submitted,
        //    Payment_cleared,
        //    Completed
        //};

        public enum DEPLOYMENT_PACKAGE_STATUS
        {
            /// <summary>
            /// chuan bi
            /// </summary>
            Preparation,
            /// <summary>
            /// Don hang san sang de dong goi
            /// </summary>
            Ready,
            /// <summary>
            /// dong goi/giao hang hoan tat. complete/shipped
            /// </summary>
            Complete
        }


        public enum TICKET_TYPE
        {
            Sales = 1,
            Finance = 2,
            NuveiOnboarding = 3,
            Deployment = 4,
            Support = 5,
            Quick = 7,
            IMSOnboarding = 10,
            Bug = 2000,
            FeatureRequest = 2010
        }
        public enum DeploymentTicket_Status
        {
            Open = 3000,
            Close =3001,
            Cancel = 3002
        }
        public enum TICKET_STATUS
        {
            Sales_Lead = 1,
            Sales_Lost = 2,
            Sales_Closed = 3,
            Finance_Open = 6,
            Finance_Failed = 7,
            Finance_Complete = 8,
            NuveiOnboarding_Open = 11,
            NuveiOnboarding_Pending = 12,
            NuveiOnboarding_Failed = 13,
            NuveiOnboarding_Cancelled = 14,
            NuveiOnboarding_Complete = 15,
            Deployment_PrepNotReady = 16,
            Deployment_RrepUpdate = 18,
            Deployment_Complete = 17,
            Deployment_Cancel = 19,
            Deployment_Shipped = 20,
            IMSOnboarding_Open = 50,
            IMSOnboarding_Waiting = 55,
            IMSOnboarding_Closed = 60,
            //IMSOnboarding_ClosedNR = 65,
            //support khong xu ly tu dong, nen khong the hien trong enum nay.
            //Support_Open = 21,
            //Support_Close = 25,
            Support_Close_NR = 30,
            Bug_Closed_NoAction = 2040,
            FeatureRequest_Closed = 2040
            //...
        }
        public static List<long> TICKET_STATUS_CLOSED = new List<long> { 3, 8, 15, 17, 60 };
        public enum LICENSES_ITEM_TYPES
        {
            ONETIME,
            SUBSCRIPTION
        };

        
        //public enum TIMEZONE_NUMBER
        //{
        //    Eastern = "-5",
        //    Central = -6,
        //    Mountain = -6,
        //    Pacific = -7,
        //};

        private static string session_webinfo = "webinfo";
        /// <summary>
        /// thong tin cong ty, he thong
        /// </summary>
        public static SystemConfiguration GetWebInfomation(bool reload = false)
        {
            if (HttpContext.Current?.Session[session_webinfo] == null || reload == true)
            {
                WebDataModel db = new WebDataModel();
                var webinfo = db.SystemConfigurations.FirstOrDefault();
                //HttpContext.Current.Session[session_webinfo] = webinfo;
                return webinfo;
            }
            else
            {
                return HttpContext.Current.Session[session_webinfo] as SystemConfiguration;
            }
        }

        /// <summary>
        /// thong tin authorize Mxmerchant
        /// </summary>
        //public static OauthInfo GetOauthInfo()
        //{
        //    WebDataModel db = new WebDataModel();
        //    var webinfo = db.SystemConfigurations.FirstOrDefault();
        //    var Info = new OauthInfo
        //    {
        //        AccessToken = webinfo.MxMerchant_AccessToken,
        //        AccessSecret = webinfo.MxMerchant_AccessSecret,
        //    };
        //    return Info;
        //}


        private static string session_department = "department_list";
        /// <summary>
        /// departmets list
        /// </summary>
        public static List<P_Department> GetDepartments(bool reload = false)
        {
            if (HttpContext.Current.Session[session_department] == null || reload == true)
            {
                WebDataModel db = new WebDataModel();
                var departments = db.P_Department.Where(d => d.Active == true).ToList();
                HttpContext.Current.Session[session_department] = departments;
                return departments;
            }
            else
            {
                return HttpContext.Current.Session[session_department] as List<P_Department>;
            }
        }

        private static string session_ticket_type = "ticket_type_list";
        /// <summary>
        /// loai ticket
        /// </summary>
        public static List<T_TicketType> GetTicketTypeList(bool reload = false)
        {
            if (HttpContext.Current.Session[session_ticket_type] == null || reload == true)
            {
                WebDataModel db = new WebDataModel();
                var ticketTypes = db.T_TicketType.Where(t => t.Active == true).OrderBy(t => t.Order).ToList();
                HttpContext.Current.Session[session_ticket_type] = ticketTypes;
                return ticketTypes;
            }
            else
            {
                return HttpContext.Current.Session[session_ticket_type] as List<T_TicketType>;
            }
        }


        private static bool _TicketSystem;
        /// <summary>
        /// switch on ticket system
        /// </summary>
        public static bool TicketSystem
        {
            get
            {
                if (HttpContext.Current.Session["ticketsystem"] == null)
                {
                    _TicketSystem = false;
                }
                else
                {
                    _TicketSystem = bool.Parse(HttpContext.Current.Session["ticketsystem"].ToString());
                }
                return _TicketSystem;
            }
            set
            {
                _TicketSystem = value;
                HttpContext.Current.Session["ticketsystem"] = _TicketSystem;
            }
        }

        public static string session_view_history = "view_history";
        /// <summary>
        /// name|value;name|value
        /// </summary>
        public static string TabHistory
        {

            get
            {
                string tabHistory = "";
                if (HttpContext.Current.Session[session_view_history] != null)
                {
                    tabHistory = HttpContext.Current.Session[session_view_history].ToString();
                }
                return tabHistory;

            }
            set
            {
                string tabHistory = "";
                if (HttpContext.Current.Session[session_view_history] != null)
                {
                    tabHistory = HttpContext.Current.Session[session_view_history].ToString().ToLower();
                }

                //string[] hisvalue = value.ToString().Split(new char[] { '|' });
                if (!tabHistory.Contains(value.ToLower()))
                {
                    tabHistory += string.IsNullOrWhiteSpace(tabHistory) == true ? value.ToLower() : ";" + value.ToLower();
                }
                else
                {
                    if (tabHistory.Contains(";" + value.ToLower()))
                    {
                        tabHistory = tabHistory.Replace(";" + value.ToLower(), "");
                    }
                    else if (tabHistory.Contains(value.ToLower() + ";"))
                    {
                        tabHistory = tabHistory.Replace(value.ToLower() + ";", "");
                    }
                    else
                    {
                        tabHistory = tabHistory.Replace(value.ToLower(), "");
                    }

                    tabHistory += string.IsNullOrWhiteSpace(tabHistory) == true ? value.ToLower() : ";" + value.ToLower();
                }

                HttpContext.Current.Session[session_view_history] = tabHistory.ToLower();

            }

        }



        public static string ScrollEffect(string description)
        {
            return description.Replace("<img", "<img data-aos=\"fade-up-right\" ").Replace("<p", "<p data-aos=\"fade-up-left\" ").Replace("<h3", "<h3 data-aos=\"fade-up-left\" ");
        }

    }
}
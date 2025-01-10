using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common
{
    public class Constants
    {
        public const string SystemName = "IMS Service";
        public const string KeyCode = "enrich@2019";
        public const string SwaggerApiDocsName = "api-docs";

        public sealed class ServiceName
        {
            public const string Company = "Enrich & CO";
            public const string Product = "Mango for Salon";
            public const string IMSTeam = "IMS Team";
            public const string POSTeam = "POS Team";
            public const string PaymemtFailedNotification = "Payment Failed Notification Service";
            public const string PaymentLaterNotification = "Payment Later Notification Service";
            public const string Recurring = "IMS Service";
            public const string SMSSender = "SMS Sender Service";
        }
        /// <summary>
        /// Alias Enumvalue table
        /// </summary>
        public static class AliasEnumValueTable
        {
            /// <summary>
            /// Extend SQL join with table EnumValue for customer source
            /// </summary>
            public const string CustomerSource = "CustomerSource";
            /// <summary>
            /// Extend SQL join with table EnumValue for customer processor
            /// </summary>
            public const string CustomerProcessor = "CustomerProcessor";
        }
        /// <summary>
        /// Unlimited value
        /// </summary>
        public const int UnlimitedValue = 10000000;

        /// <summary>
        /// Using in condition to check unlimited service
        /// </summary>
        public const int UnlimitedNumber = 1000000;

        /// <summary>
        /// { ";", "," }
        /// </summary>
        //public static readonly string[] SEPARATOR_CHARACTERS = { ";", "," };
        public static readonly char[] SEPARATOR_CHARACTERS = { ';', ',' };

        public const int DEFAULT_PAGE_SIZE = 20;

        public const int DEFAULT_QUICK_SEARCH_ITEMS = 50;

        public const string VatValueList = "4,6.0,6%-3,10.0,10%-1,12.0,12%-2,21.0,21%";

        public sealed class Format
        {
            //todo NEW_LANGUAGE: DateFormat
            public const string Date_EN = "MM/dd/yyyy";
            public const string Date_FR = "dd/MM/yyyy";
            public const string Date_NL = "dd/MM/yyyy";
            public const string Date_DE = "dd/MM/yyyy";
            public const string Date_yyyyMMdd = "yyyy-MM-dd";
            public const string Date_MMddyyyy_HHmmss = "MM/dd/yyyy HH:mm:ss";
            public const string Date_MMMddyyyy_HHmm = "MMM dd, yyyy HH:mm";
            public const string Date_MMMddyyyy = "MMM dd, yyyy";
            public const string Date_yyMM = "yyMM";
            public const string Date_fff = "fff";
            public const string Date_yyMMdd = "yyMMdd";
            public const string Date_yyMMddhhmmssff = "yyMMddhhmmssff";
            public const string Date_yyMMddhhmmssfff = "yyMMddhhmmssfff";
            public const string Date_ddMMyyyy_HHmmss = "dd/MM/yyyy HH:mm:ss";
            public const string Date_Full = "yyyy-MM-dd HH:mm:ss.fff";

            public const string CurrencyDollar = "$#,##0.#0";
        }

        public static class CloudUploadPath
        {
            public static string Default = "public/enrich/default/";
        }

        public sealed class Unit
        {
            public const string CurrencyEuro = "€";

            public const string CurrencyDollar = "$";

            public const string Picoliter = "pl";

            public const string Meter = "m";

            public const string MeterSquare = "m²";

            public const string MeterCubic = "m³";

            public const string Percent = "%";

            public const string Energy = "kWh/m²/j";

            public const string Litre = "l";

            public const string Kilogram = "kg";

            public static bool IsCurrency(string unit) => unit == CurrencyEuro || unit == CurrencyDollar;
        }

        public sealed class RegexPattern
        {
            public const string Sleeping = @"(\s*((\d{1,9}\s+){0,1}))";
        }

        public sealed class CacheName
        {
            public const string NoCache = "NoCache";

            public const string MemoryCache = "MemoryCache";

            public const string StaticCache = "StaticCache";

            public const string RedisCache = "RedisCache";
        }

        public sealed class FlanderRegistrationRight
        {
            /// <summary>
            /// "7% (standaard tarief)"
            /// </summary>
            public static int RegistrationTypeFlander7 = 9000;
            /// <summary>
            /// 6% (energetische renovatie)
            /// </summary>
            public static int RegistrationTypeFlander6 = 9001;
            /// <summary>
            /// 1% (onroerend erfgoed)
            /// </summary>
            public static int RegistrationTypeFlander1 = 9002;

            public static double FlanderBasePrice = 200000d;
            public static double FlanderCoreAndEdgeCitiesBasePrice = 220000d;
            public static double FlanderDiscountAmountForFamilyHouse = 80000d;
            public static string FlanderCoreAndEdgeZipCode = @"1500;1501;1502;1540;1541;1560;1570;1600;1601;1602;1620;1630;1640;1650;1651;1652;1653;1654;1700;1701;1702;1703;1730;1731;1740;1741;1742;1745;1750;1755;1760;1761;1770;1780;1785;1790;1800;1800;1820;1830;1831;1840;1850;1851;1852;1853;1860;1861;1880;1910;1930;1932;1933;1950;1970;1980;1981;1982;2000;2018;2020;2030;2040;2050;2060;2100;2140;2170;2180;2300;2600;2610;2660;2800;2801;2811;2812;2850;3000;3001;3010;3012;3018;3040;3060;3061;3070;3071;3078;3080;3090;3500;3501;3510;3511;3512;3600;8000;8200;8310;8380;8400;8500;8501;8510;8511;8800;8970;8972;8978;9000;9030;9031;9032;9040;9041;9042;9050;9051;9052;9100;9111;9112;9200;9300;9308;9310;9320";

            public static int[] FlanderRegistrationTypeSystemIds = new int[] { RegistrationTypeFlander7, RegistrationTypeFlander6, RegistrationTypeFlander1 };
            public static string FlanderRegistrationRuleUrl = @"https://belastingen.vlaanderen.be/aanpassing-verkooprecht-vanaf-1-juni-2018";
        }

        public sealed class Appointment
        {
            public const string EndOfCommentFeedbackKey = "*********";
        }

        public sealed class PersonErrorMessage
        {
            public const string PersonHasNoEmail = "PERSON_HAS_NO_EMAIL";
            public const string PersonNeverSendEmail = "PERSON_NEVER_SEND_EMAIL";
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System.Net;

namespace EnrichcousBackOffice.AppLB
{
    public class CommonFunc
    {


        /// <summary>
        /// chuyen doi ngay dd/MM/yyyy sang MM/dd/yyyy
        /// </summary>
        /// <param name="datevi">dd/MM/yyyy</param>
        /// <returns></returns>
        public static DateTime DateVi_To_DateEn(string datevi)
        {
            var dateViArr = datevi.Split(new char[] { '/' });
            string dateEn = string.Join("/", dateViArr[1], dateViArr[0], dateViArr[2]);
            return DateTime.Parse(dateEn);
        }

        /// <summary>
        /// Ham tra ve ngay dau, cuoi thang theo ngay bat ky
        /// </summary>
        /// <param name="currentDate">Ngay bat ky trong thang</param>
        /// <param name="BeginDateOfMonth">Tra ve ngay dau tien cua thang</param>
        /// <param name="EndDateOfMonth">Tra ve ngay cuoi cung cua thang</param>
        public static void GetBeginEndDateOfMonth(DateTime currentDate, out DateTime BeginDateOfMonth, out DateTime EndDateOfMonth)
        {
            string bDate = currentDate.Month.ToString() + "/1/" + currentDate.Year.ToString() + " 0:0:0";
            string eDate = currentDate.Month.ToString() + "/" + DateTime.DaysInMonth(currentDate.Year, currentDate.Month).ToString() + "/" + currentDate.Year.ToString() + " 23:59:59";
            DateTime.TryParse(bDate, out DateTime beginDate);
            DateTime.TryParse(eDate, out DateTime endDate);
            BeginDateOfMonth = beginDate;
            EndDateOfMonth = endDate;
        }

        /// <summary>
        /// Ham tra ve ngay dau tien(Monday) va ngay cuoi tuan(Sunday)
        /// </summary>
        /// <param name="currentDate">Ngay bat ky trong tuan</param>
        /// <param name="beginWeek">Tra ve ngay dau tien</param>
        /// <param name="endWeek">Tra ve ngay cuoi tuan</param>
        public static void GetBeginEndWeek(DateTime currentDate, out DateTime beginWeek, out DateTime endWeek)
        {

            switch (currentDate.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    beginWeek = currentDate.AddDays(-4);
                    endWeek = currentDate.AddDays(2);
                    break;
                case DayOfWeek.Monday:
                    beginWeek = currentDate;
                    endWeek = currentDate.AddDays(6);
                    break;
                case DayOfWeek.Saturday:
                    beginWeek = currentDate.AddDays(-5);
                    endWeek = currentDate.AddDays(1);
                    break;
                case DayOfWeek.Sunday:
                    beginWeek = currentDate.AddDays(-6);
                    endWeek = currentDate;
                    break;
                case DayOfWeek.Thursday:
                    beginWeek = currentDate.AddDays(-3);
                    endWeek = currentDate.AddDays(3);
                    break;
                case DayOfWeek.Tuesday:
                    beginWeek = currentDate.AddDays(-1);
                    endWeek = currentDate.AddDays(5);
                    break;
                case DayOfWeek.Wednesday:
                    beginWeek = currentDate.AddDays(-2);
                    endWeek = currentDate.AddDays(4);
                    break;
                default:
                    beginWeek = currentDate;
                    endWeek = currentDate;
                    break;
            }
        }


        /// <summary>
        /// chuyen doi cac chu cai tieng viet co dau sang khong dau
        /// </summary>
        /// <param name="inputS"></param>
        /// <returns></returns>
        public static string ConvertNonUnicodeURL(string inputS, bool include_space = false)
        {
            string outputS = "";
            if (string.IsNullOrWhiteSpace(inputS) == false)
            {
                var arrString = inputS.Split(new char[] { });
                outputS = Regex.Replace(inputS, "á|à|ả|ã|ạ|â|ă|ẩ|ẫ|ậ|ầ|ấ|ă|ằ|ắ|ẳ|ẵ|ặ", "a");
                outputS = Regex.Replace(outputS, "Á|À|Ả|Ã|Ạ|Â|Ấ|Ầ|Ẩ|Ẫ|Ậ|Ă|Ắ|Ằ|Ẳ|Ẵ|Ặ", "A");
                outputS = Regex.Replace(outputS, "ó|ò|ỏ|õ|ọ|ô|ố|ồ|ổ|ỗ|ộ|ơ|ớ|ờ|ở|ỡ|ợ", "o");
                outputS = Regex.Replace(outputS, "Ó|Ò|Ỏ|Õ|Ọ|Ô|Ố|Ồ|Ổ|Ỗ|Ộ|Ơ|Ớ|Ờ|Ở|Ỡ|Ợ", "O");
                outputS = Regex.Replace(outputS, "é|è|ẻ|ẽ|ẹ|ê|ế|ề|ể|ễ|ệ", "e");
                outputS = Regex.Replace(outputS, "É|È|Ẻ|Ẽ|Ẹ|Ê|Ế|Ề|Ể|Ễ|Ệ", "E");
                outputS = Regex.Replace(outputS, "ú|ù|ủ|ũ|ụ|ư|ứ|ừ|ử|ữ|ự", "u");
                outputS = Regex.Replace(outputS, "Ú|Ù|Ủ|Ũ|Ụ|Ư|Ứ|Ừ|Ử|Ữ|Ự", "U");
                outputS = Regex.Replace(outputS, "í|ì|ỉ|ĩ|ị", "i");
                outputS = Regex.Replace(outputS, "Í|Ì|Ỉ|Ĩ|Ị", "U");
                outputS = Regex.Replace(outputS, "đ", "d");
                outputS = Regex.Replace(outputS, "Đ", "D");
            }

            outputS = Regex.Replace(outputS, "[?&/#'@[\\]()*^!`~]", "-");
            outputS = outputS.Replace("//", "-");
            if (!include_space)
            {
                outputS = outputS.Replace(" ", "-");
                outputS = Regex.Replace(outputS, "-+", "-");
            }
            else
            {
                outputS = Regex.Replace(outputS, "-+", " ");
            }

            return outputS;
        }
        public static Random random = new Random();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxCode">Current max full code</param>
        /// <param name="head">head chars of code</param>
        /// <returns></returns>

        public static string RenderCodeId(string maxCode, string head)
        {

            var now = DateTime.UtcNow;
            var number = random.Next(1, 99);
            if (!string.IsNullOrEmpty(maxCode) && now.ToString("yyMM") == maxCode.Substring(head.Length, 4))
            {
                if (maxCode.Length < 4 && string.IsNullOrEmpty(head))
                {
                    maxCode = maxCode.PadLeft(4, '0');
                }
                int index = int.Parse(maxCode.Substring(head.Length + 4, maxCode.Length - 6 - head.Length)) + 1;
                return head + now.ToString("yyMM") + index.ToString().PadLeft(4, '0') + number.ToString().PadLeft(2, '0'); //now.ToString("ff");
            }
            else
            {
                return head + now.ToString("yyMM") + "0001" + number.ToString().PadLeft(2, '0');
            }
        }

        public static bool compareName(string input1, string input2)
        {
            return ConvertNonUnicodeURL(input1).Replace("-", "") == ConvertNonUnicodeURL(input2).Replace("-", "");
        }
        public static bool SearchName(string name, string text, bool oneway = true)
        {
            string a = ConvertNonUnicodeURL(name?.ToLower()).Replace("-", "").Replace("'", "").Replace(".", "").ToLower();
            string b = ConvertNonUnicodeURL(text?.ToLower()).Replace("-", "").Replace("'", "").Replace(".", "").ToLower();
            if (oneway)
            {
                return a.Contains(b);
            }

            return a.Contains(b) || b.Contains(a);
        }


        /// <summary>
        /// Search by word in SearchName
        /// </summary>
        /// <param name="name">content to search</param>
        /// <param name="content">original content</param>
        /// <returns></returns>
        public static bool SearchNameByElement(string name, string content)
        {
            try
            {
                content = ConvertNonUnicodeURL(content)?.ToLower().Replace("-", "").Replace("'", "").Replace(".", "");
                var element_search = name?.Trim()?.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in element_search)
                {
                    string name_search = ConvertNonUnicodeURL(item)?.ToLower().Replace("-", "").Replace("'", "").Replace(".", "");
                    if (string.IsNullOrWhiteSpace(content) == false && (content.Contains(name_search) == true || name_search.Contains(content) == true))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// xoa the image trong content
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        public static string RemoveImageTag(string _str)
        {
            while (_str.IndexOf("<img") != -1)
            {
                int start = _str.IndexOf("<img"); //lay vi tri the mo
                string str_first = _str.Substring(0, start); //lay phan noi dung truoc the mo

                string str_last = _str.Substring(start); //lay phan noi dung sau the mo
                int end = str_last.IndexOf("/>"); //lay vi tri the dong (bat dau tu phan noi dung sau the mo)
                str_last = str_last.Substring(end + 2); //lay phan noi dung sau the dong

                _str = str_first + str_last; //chuoi moi = phan noi dung truoc the mo + phan noi dung sau the dong
            }

            return _str;
        }


        /// <summary>
        /// tinh thong so page va recordsperpage de phan trang
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rpp"></param>
        /// <param name="totalRecords"></param>
        /// <param name="_page"></param>
        /// <param name="_rpp"></param>
        public static void PagedList(int? page, int? rpp, int totalRecords, out int _page, out int _rpp, out int skip, out int take)
        {
            _page = page ?? 1;
            _rpp = rpp ?? 15;
            //check page
            if (_page > totalRecords / _rpp && totalRecords % _rpp == 0)
            {
                _page = totalRecords / _rpp;

            }
            else if (_page > totalRecords / _rpp + 1)
            {
                _page = totalRecords / _rpp + 1;
            }

            if (_page < 1)
            {
                _page = 1;
            }

            //check take
            if (_rpp * _page > totalRecords)
            {
                take = totalRecords - (_rpp * (_page - 1));
            }
            else
            {
                take = _rpp;
            }
            skip = (_page - 1) * _rpp;

            return;
        }


        /// <summary>
        /// date remain
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DateTimeRemain(DateTime dt)
        {
            TimeSpan ts = (DateTime.UtcNow - dt);
            var result = "";
            if (ts.Days == 0 && ts.Hours == 0 && ts.Minutes == 0)
            {
                result= "a few seconds";
            }
            else if (ts.Days == 0 && ts.Hours == 0)
            {
                result= ts.Minutes + " minutes";
            }
            else if (ts.Days == 0)
            {
                result= ts.Hours + " hours " + ts.Minutes + " minutes";
            }
            else if (ts.Days > 365)
            {
                result= Math.Round((decimal)(ts.Days / 365), 0, MidpointRounding.ToEven).ToString() + " years";
            }
            else if (ts.Days > 30)
            {
                result= Math.Round((decimal)(ts.Days / 30), 0, MidpointRounding.ToEven).ToString() + " months";
            }
            else
            {
                result = ts.Days + " days";// +ts.Hours + " hours " + ts.Minutes + " minutes ago";
            }
            return result + " ago";
        }
        /// <summary>
        /// date remain
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DateTimeNextRemain(DateTime dt)
        {
            TimeSpan ts = dt - DateTime.UtcNow;
            var result = "";
            if (ts.Days == 0 && ts.Hours == 0 && ts.Minutes == 0)
            {
                result = "a few seconds";
            }
            else if (ts.Days == 0 && ts.Hours == 0)
            {
                result = ts.Minutes + " minutes";
            }
            else if (ts.Days == 0)
            {
                result = ts.Hours + " hours " + ts.Minutes + " minutes";
            }
            else if (ts.Days > 365)
            {
                result = Math.Round((decimal)(ts.Days / 365), 0, MidpointRounding.ToEven).ToString() + " years";
            }
            else if (ts.Days > 30)
            {
                result = Math.Round((decimal)(ts.Days / 30), 0, MidpointRounding.ToEven).ToString() + " months";
            }
            else
            {
                result = ts.Days + " days";// +ts.Hours + " hours " + ts.Minutes + " minutes ago";
            }
            return result;
        }
        /// <summary>
        ///  remain date for Subscription License
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string LicenseRemainingTime(DateTime dt)
        {
            int days = (dt - DateTime.UtcNow).Days;
            if (days < 0)
            {
                return "Expires";
            }
            string result = "";
            //if (days > 365)
            //{
            //     int year = (int)(days / 365);
            //     days -= (365 * year);
            //     result += year + " years ";
            //}
            if (days > 365 * 10) //lớn hơn 10 năm
            {
                return "Life time";
            }
            if (days > 30)
            {
                int month = (days / 30);
                days -= (30 * month);
                result += month + " months ";
            }
            return result += days + " days";
        }
        public static bool IsValidEmail(string emailAddress)
        {
            try
            {
                return Regex.IsMatch(emailAddress,
                @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        public static string GetIDStr(int id)
        {
            string result = "";
            if (id < 10)
            {
                result = "000" + id.ToString();

            }
            else if (id < 100)
            {
                result = "00" + id.ToString();
            }
            else if (id < 1000)
            {
                result = "0" + id.ToString();
            }
            else
            {
                result = id.ToString();
            }
            return result;
        }

        public static string GetCardTypeByCardNumber(string cardNumber)
        {
            // 'American Express       34, 37            15
            //'Discover               6011              16
            //'Master Card            51 to 55          16
            //'Visa                   4                 13, 16

            //visa 
            Regex checkVisa = new Regex("^(?:4[0-9]{12}(?:[0-9]{3})?)$");
            if (checkVisa.IsMatch(cardNumber) == true)
            {
                return "visa";
            }
            //amex
            Regex checkAmex = new Regex("^(3[47][0-9]{13})$");
            if (checkAmex.IsMatch(cardNumber) == true)
            {
                return "amex";
            }
            //discover
            Regex checkDiscover = new Regex("^(6011[0-9]{12})$");
            if (checkDiscover.IsMatch(cardNumber))
            {
                return "discover";
            }
            //mc
            Regex checkMC = new Regex("^((?:51|55)[0-9]{14})$");
            if (checkMC.IsMatch(cardNumber))
            {
                return "mastercard";
            }

            return "Other";
        }



        /// <summary>
        /// save image from string 64bit
        /// </summary>
        /// <param name="stringImage"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Boolean SaveBinaryImage(string stringImage, string filename)
        {

            var appPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            string path = Path.Combine(appPath, @"Upload\BinaryImage\");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(stringImage)))
                {
                    using (System.Drawing.Bitmap bm2 = new System.Drawing.Bitmap(ms))
                    {
                        bm2.Save(path + filename);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static string GetLastRowString(string st, char charsplit)
        {
            string[] st_s = st.Split(charsplit);
            for (int i = st_s.Length - 1; i > 0; i--)
            {
                if (!string.IsNullOrEmpty(st_s[i]))
                    return st_s[i];
            }
            return "";
        }

        /// <summary>
        /// loai bo cac ky tu khong phai la phone number
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static string CleanPhone(string phone)
        {
            try
            {
                Regex digitsOnly = new Regex(@"[^\d]");
                return digitsOnly.Replace(phone, "");
            }
            catch (Exception)
            {
                return "";
            }

        }

        /// <summary>
        /// kiem tra ton tai phan tu strArr1 trong strArr2
        /// </summary>
        /// <param name="strArr1"></param>
        /// <param name="strArr2"></param>
        /// <returns>phan tu duoc tim thay</returns>
        public static List<string> CheckMatchList(List<string> strArr1, List<string> strArr2)
        {
            List<string> matchItems = new List<string>();
            foreach (var i1 in strArr1)
            {
                if (strArr2.Contains(i1))
                {
                    matchItems.Add(i1);
                }
            }
            return matchItems;
        }
        /// <summary>
        /// kiem tra ton tai phan tu strArr1 trong strArr2
        /// </summary>
        /// <param name="strArr1"></param>
        /// <param name="strArr2"></param>
        /// <returns>phan tu duoc tim thay</returns>
        public static string RenderRazorViewToString(string viewName, object model, ControllerBase cb)
        {
            cb.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(cb.ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(cb.ControllerContext, viewResult.View,
                                             cb.ViewData, cb.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(cb.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
        public static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }


        /// <summary>
        /// Doi so tien => chu
        /// </summary>
        public class SpellMoney
        {

            static string[] chuso_19 = new string[]{ "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nice", "Ten",
                "Eleven", "Twelve", "Thirteen","Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            static string[] tram_nghin_trieu = new string[] { "Hundred", "Thousand", "Milion", "Billion" };
            static string[] hang_chuc_20_90 = new string[] { "Twenty", "Thirty", "Forty", "Fifty", "Sixties", "Seventies", "Eighty", "Ninety" };

            /// <summary>
            /// 
            /// </summary>
            /// <param name="amount"></param>
            /// <returns></returns>
            public static string SpellOutAmount_USD(double amount)
            {
                string amount_str = amount.ToString("#,##0.#0");
                string left_number = (amount_str.Split(new char[] { '.' })[0])?.Replace(",", "");
                string right_number = amount_str.Split(new char[] { '.' })[1];

                string left_spell_number = "";
                //xet left number
                int left_number_amount = int.Parse(left_number);
                if (left_number_amount <= 100)
                {
                    left_spell_number = Spell0_100(left_number_amount);
                }
                else if (left_number_amount <= 1000)
                {
                    left_spell_number = Spell1000(left_number_amount);
                }
                else if (left_number_amount <= 10000)
                {
                    left_spell_number = Spell10000(left_number_amount);
                }
                else if (left_number_amount <= 100000)
                {
                    left_spell_number = Spell100000(left_number_amount);
                }
                else if (left_number_amount <= 1000000)
                {
                    left_spell_number = Spell1000000(left_number_amount);
                }
                else
                {
                    //chi tinh toan den con so 1trieu usd.
                    return "N/A";
                }
                left_spell_number += int.Parse(left_number) == 1 ? " Dollar" : " Dollars";
                //xet right number
                string right_spell_number = " ";
                if (!string.IsNullOrWhiteSpace(right_number) && int.Parse(right_number) > 0)
                {
                    right_spell_number += Spell0_100(int.Parse(right_number));
                    right_spell_number += (int.Parse(right_number) > 1 ? " Cent" : "Cents");
                }

                return (left_spell_number == " Dollars" ? "" : left_spell_number) + right_spell_number;
            }

            /// <summary>
            /// <= 100
            /// </summary>
            /// <param name="number"></param>
            /// <returns></returns>
            private static string Spell0_100(int number)
            {
                string result = "";
                if (number == 100)
                {
                    result = "One " + tram_nghin_trieu[0];
                    return result;
                }
                else
                {
                    if (number <= 19 && number > 0)
                    {
                        result += chuso_19[number - 1];
                    }
                    else if (number > 19)
                    {
                        string right_1 = number.ToString().Substring(0, 1);
                        string right_2 = number.ToString().Substring(1);
                        result += hang_chuc_20_90[int.Parse(right_1) - 2];
                        if (int.Parse(right_2) > 0)
                        {
                            result += "-" + chuso_19[int.Parse(right_2) - 1];
                        }


                    }
                }

                return result;
            }

            /// <summary>
            /// >100 & <=1000
            /// </summary>
            /// <param name="number"></param>
            /// <returns></returns>
            private static string Spell1000(int number)
            {
                if (number == 1000)
                {
                    return "One " + tram_nghin_trieu[1];
                }
                else if (number > 0)
                {
                    int so_dau_tien = int.Parse(number.ToString().Substring(0, 1));
                    return chuso_19[so_dau_tien - 1] + " " + tram_nghin_trieu[0] + " " + Spell0_100(int.Parse(number.ToString().Substring(1)));
                }
                return "";
            }


            /// <summary>
            /// >1000 & <=10000
            /// </summary>
            /// <param name="number"></param>
            /// <returns></returns>
            private static string Spell10000(int number)
            {
                if (number == 10000)
                {
                    return "Ten " + tram_nghin_trieu[1];
                }
                else if (number > 0)
                {
                    int so_dau_tien = int.Parse(number.ToString().Substring(0, 1));
                    return chuso_19[so_dau_tien - 1] + " " + tram_nghin_trieu[1] + " " + Spell1000(int.Parse(number.ToString().Substring(1)));
                }
                return "";
            }

            /// <summary>
            /// >10000 & <=100000
            /// </summary>
            /// <param name="number"></param>
            /// <returns></returns>
            private static string Spell100000(int number)
            {
                if (number == 100000)
                {
                    return "One " + tram_nghin_trieu[0] + tram_nghin_trieu[1] + " " + tram_nghin_trieu[1];
                }
                else if (number > 0)
                {
                    int haiso_dau_tien = int.Parse(number.ToString().Substring(0, 2));
                    return Spell0_100(haiso_dau_tien) + " " + tram_nghin_trieu[1] + " " + Spell10000(int.Parse(number.ToString().Substring(2)));
                }
                return "";
            }

            /// <summary>
            /// >100000 & <=1000000
            /// </summary>
            /// <param name="number"></param>
            /// <returns></returns>
            private static string Spell1000000(int number)
            {
                if (number == 1000000)
                {
                    return "One " + tram_nghin_trieu[2];
                }
                else if (number > 0)
                {
                    int so_dau_tien = int.Parse(number.ToString().Substring(0, 1));
                    var result = Spell0_100(so_dau_tien) + " " + tram_nghin_trieu[0];
                    string result100000 = Spell100000(int.Parse(number.ToString().Substring(1)));
                    if (!string.IsNullOrWhiteSpace(result100000))
                    {
                        result += " " + result100000;
                    }
                    return result;

                }
                return "";

            }


        }
        public static string getStringValid(params string[] strings) => strings.FirstOrDefault(s => !string.IsNullOrEmpty(s));
        /// <summary>
        /// Convert ticket id to view ticket id ####.######
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string view_TicketId(long? id)
        {
            return id != null ? id.ToString().Insert(4, ".") : "";
        }

        /// <summary>
        /// Fill data object into input template pdf
        /// </summary>
        /// <param name="pdfTemplate">path of pdf file template</param>
        /// <param name="savepath">path to save filled file</param>
        /// <param name="data">data object to fill</param>
        /// <param name="filename">Save name file filled</param>
        /// <param name="date_tail">add 'yyMMdd' at tail file name</param>
        /// <returns>string file path pdf already filled</returns>
        public static string FillPdf(string pdfTemplate, string savepath, object data, string filename, bool date_tail = true, bool Flattening = false)
        {
            if (date_tail)
            {
                filename = filename + "_" + DateTime.UtcNow.ToString("yyMMdd");
            }

            DirectoryInfo d = new DirectoryInfo(HttpContext.Current.Server.MapPath(savepath));
            if (!d.Exists)
            {
                d.Create();
            }
            else
            {
                var n = d.GetFiles(filename + "*.pdf").Length + 1;
                filename += "_" + n;
            }
            string filepath = savepath + "/" + filename + ".pdf";
            string newFile = Path.Combine(HttpContext.Current.Server.MapPath(filepath));
            PdfReader pdfReader = new PdfReader(pdfTemplate);
            // PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
            using (PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create)))
            {
                AcroFields pdfFormFields = pdfStamper.AcroFields;
                var json = JsonConvert.SerializeObject(data);
                json = json.Replace("true", "\"Yes\"").Replace("false", "\"No\"");
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (var item in dictionary)
                {
                    pdfFormFields.SetField(item.Key, item.Value);
                }
                pdfStamper.FormFlattening = Flattening;

            }
            pdfReader.Close();
            return filepath;
        }

        //public static DateTime? EndMonth(DateTime? dateTime)
        //{
        //    if (dateTime == null) { return null; }
        //    else
        //    {
        //        return new DateTime(dateTime.Value.Year, dateTime.Value.Month, DateTime.DaysInMonth(dateTime.Value.Year, dateTime.Value.Month));
        //    }
        //}

       public static string FormatDateRemain(DateTime dt)
        {
            string time = dt > DateTime.UtcNow ? "later" : "ago";
            TimeSpan ts = dt > DateTime.UtcNow ? dt - DateTime.UtcNow : DateTime.UtcNow - dt;

            if (ts.Days == 0 && ts.Hours == 0 && ts.Minutes == 0)
            {
                return "a few seconds " + time;
            }
            else if (ts.Days == 0 && ts.Hours == 0)
            {
                return ts.Minutes + " minutes " + time;
            }
            else if (ts.Days == 0)
            {
                return ts.Hours + " hours " + ts.Minutes + " minutes " + time;
            }
            else if (ts.Days > 365)
            {
                return Math.Round((decimal)(ts.Days / 365), 0, MidpointRounding.ToEven).ToString() + " years " + time;
            }
            else if (ts.Days > 30)
            {
                return Math.Round((decimal)(ts.Days / 30), 0, MidpointRounding.ToEven).ToString() + " months " + time;
            }
            return ts.Days + " days " + time;// +ts.Hours + " hours " + ts.Minutes + " minutes ago";
        }

       public static int? FormatNumberRemainDate(int? RemainingDateNumber)
       {

            //  expires date +1
            return (RemainingDateNumber != null ? (RemainingDateNumber + 1) : null);
       }
        public static DateTime ConvertToUtc(DateTime datetime,int TimezoneNumber)
        {
            return datetime.AddHours(-TimezoneNumber);
        }
        public static DateTime ConvertToSpecificTime(DateTime datetime, int TimezoneNumber)
        {
            return datetime.AddHours(TimezoneNumber);
        }
    }
}
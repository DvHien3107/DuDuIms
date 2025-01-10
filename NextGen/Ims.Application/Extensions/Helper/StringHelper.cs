using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Promotion.Application.Extensions
{
    public static class StringHelper
    {
        #region Regex 
        public static bool IsEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            return match.Success;
        }

        public static bool IsPhone(this string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }
            string[] areaCode = new string[] { "032", "033", "034", "035", "036", "037",
            "038", "039", "070", "079", "077","076","078", "083", "084","085","081","082","056","058","059" };

            bool result = areaCode.Any(x => phone.StartsWith(x));
            return result;
        }
        public static bool IsNumber(this string number)
        {
            try
            {
                Convert.ToDecimal(number);
                return true;
            }
            catch { }
            return false;
        }
        public static bool IsPasswordValid(this string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                // messege = "password rỗng";
                return false;
            }
            Regex regex = new Regex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$");
            Match match = regex.Match(password);
            var result = match.Success;
            //  messege = result ? "" : "Tối thiểu tám ký tự, ít nhất một chữ cái viết hoa, viết thường và một số";
            return result;
        }

        public static string RemoveAccents(this string input)
        {
            string result = new string(input
                .Normalize(System.Text.NormalizationForm.FormD)
                .ToCharArray()
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
            return result;
            // the normalization to FormD splits accented letters in letters+accents
            // the rest removes those accents (and other non-spacing characters)
            // and creates a new string from the remaining chars
        }

        public static string RegexSpecialChar(this string text)
        {
            return Regex.Replace(text, @"(\s+|@|&|'|\(|\)|<|>|#)", "");
        }
        public static string FriendlyUrl(this string strTitle)
        {
            return ReplaceSpecial(strTitle);
        }

        public static string ReplaceSpecial(string title)
        {
            if (title == null) return string.Empty;

            const int maxlen = 500;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                    c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(c.ToString().RemoveAccents());//RemapInternationalCharToAscii
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        private static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("áàảãạăắằẳẵặâấầẩẫậ".Contains(s))
            {
                return "a";
            }
            else if ("éèẻẽẹêếềểễệ".Contains(s))
            {
                return "e";
            }
            else if ("íìỉĩị".Contains(s))
            {
                return "i";
            }
            else if ("óòỏõọôốồổỗộơớờởỡợ".Contains(s))
            {
                return "o";
            }
            else if ("úùủũụưứừửữự".Contains(s))
            {
                return "u";
            }
            else if ("ýỳỷỹỵ".Contains(s))
            {
                return "y";
            }
            else if ("đ".Contains(s))
            {
                return "d";
            }
            else
            {
                return "";
            }
        }

        #endregion
        public static string RemoveUnicode(this string text)
        {
            try
            {
                if (text.Trim().Length > 0)
                {
                    string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                "đ",
                "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                "í","ì","ỉ","ĩ","ị",
                "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                "ý","ỳ","ỷ","ỹ","ỵ",};
                    string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                "d",
                "e","e","e","e","e","e","e","e","e","e","e",
                "i","i","i","i","i",
                "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                "u","u","u","u","u","u","u","u","u","u","u",
                "y","y","y","y","y",};
                    for (int i = 0; i < arr1.Length; i++)
                    {
                        text = text.Replace(arr1[i], arr2[i]);
                        text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
                    }
                }
            }
            catch
            {
                text = "";
            }


            return text;
        }
        public static string ReplaceLast(this string text, string charaters = "")
        {
            if (string.IsNullOrEmpty(charaters))
            {
                return text;
            }

            var index = text.LastIndexOf(charaters);
            return text.Remove(index, charaters.Length);

        }

        public static void WriteLog(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\Review_" + DateTime.UtcNow.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
                }
            }
        }
        public static void WriteLog(string title, string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\Review_" + title + DateTime.UtcNow.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
                }
            }
        }

        public static List<string> GetNameSpaces()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass).Select(x => x.Namespace).ToList();
        }

        #region Cryto
        public static string CreateMD5Hash(this string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string Encrypt(string clearText, string key)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText, string key)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string EncryptUrl(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptUrl(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string EncryptParamUrl(string key, string encryptParams)
        {
            return HttpUtility.UrlEncode(EncryptUrl(key, encryptParams));
        }

        public static List<string> DecrypParamUrl(string key, string strDecrypt)
        {
            string descyptString = DecryptUrl(key, strDecrypt);
            List<string> result = descyptString.Split(',').ToList();
            return result;
        }
        #endregion End Cryto
        public static string FormatDisplayDate(this DateTime? value)
        {
            if (value.HasValue)
            {
                string day = value.Value.Day.ToString();
                string month = value.Value.Month.ToString();
                string year = value.Value.ToString("yy");

                day = day.Length == 1 ? "0" + day : day;

                year = year.Length == 1 ? "0" + year : year;

                string format = $"{day} Th{month} {year}";

                return format;
            }
            else
            {
                return "";
            }
        }

        public static string FormatDisplayTotalView(this string value)
        {
            int view = Convert.ToInt32(value ?? "0");

            if (view > 999)
            {
                int firstView = 0;

                string secondView = "";

                if (value.Length == 4 || value.Length == 5)
                {
                    firstView = view / 1000;

                    secondView = "k";
                }
                else if (value.Length == 6 || value.Length == 7 || value.Length == 8)
                {
                    firstView = view / 100000;

                    secondView = "tr";
                }
                else if (value.Length >= 9)
                {
                    firstView = view / 100000;

                    secondView = "tỉ";
                }

                return $"{firstView}{secondView} lượt xem";
            }
            else
            {
                return $"{view} lượt xem";
            }
        }

        public static string FormatDisplayTimeRead(this int lengthContent)
        {
            int timeReadMinute = Convert.ToInt32(Math.Floor(Convert.ToDecimal(lengthContent / 1000)));

            return $"{timeReadMinute} phút đọc"; ;
        }

        public static string FormatDisplayDescription(this string description)
        {
            string formatDescription = description ?? "";

            if (formatDescription.Length > 150)
            {
                formatDescription = formatDescription.Substring(0, 150);
            }

            return formatDescription;
        }

        public static string CreatePin()
        {
            Random r = new Random();
            var pin = r.Next(001000, 999999).ToString("D6");
            return pin;
        }

        public static bool ContainsUnicodeCharacter(string input)
        {
            string regex = @"[àÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬđĐèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆìÌỉỈĩĨíÍịỊòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰỳỲỷỶỹỸýÝỵỴ]";

            bool res = Regex.IsMatch(input, regex);
            return res;
        }

        public static string ConvertSql(this string text)
        {
            Regex regex = new Regex(@"\'+");
            return regex.Replace(text, "''");
        }
        public static string RegexUrl(this string text)
        {
            var tmp = "";
            Regex regex = new Regex(@"\++");
            Regex regex2 = new Regex(@"\ +");
            Regex regex3 = new Regex(@"\#+");
            Regex regex4 = new Regex(@"\&+");
            Regex regex5 = new Regex(@"\.+");
            Regex regex6 = new Regex(@"\-+");
            Regex regex7 = new Regex(@"\'+");
            Regex regex8 = new Regex(@"\*+");

            tmp = regex.Replace(text, "%2B");
            tmp = regex2.Replace(text, "%20");
            tmp = regex3.Replace(text, "%23");
            tmp = regex4.Replace(text, "%26");
            tmp = regex5.Replace(text, "%2E");
            tmp = regex6.Replace(text, "%2D");
            tmp = regex7.Replace(text, "%2C");
            tmp = regex8.Replace(text, "%2A");
            return tmp;
        }

        public static string ReplacePhone(this string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return phone;
            }
            phone = phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            return phone;
        }

        public static string FormatPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return "";
            if (phoneNumber.Length == 10)
            {
                return String.Format("{0:(" + (phoneNumber.IndexOf("0") > -1 ? "0" : "") + "##) ###-####}", Convert.ToInt64(phoneNumber ?? "0"));
            }
            else
            {
                return phoneNumber;
            }
        }

        public static string FormatHidePhoneNumber(this string phoneNumber)
        {
            if (phoneNumber.Length == 10)
            {
                return string.Format("(XXX)-XXX-{0}", phoneNumber.Substring(6, 4));
            }
            else
            {
                return phoneNumber;
            }
        }

        public static string ConvertMobiNumber(string mobiNumber, string telInput = "+1")
        {
            string _mobileNumber = CleanNumber(mobiNumber);

            // trim any leading zeros
            _mobileNumber = _mobileNumber.TrimStart(new char[] { '0' });

            // check for this in case they've entered 44 (0)xxxxxxxxx or similar
            if (_mobileNumber.StartsWith("+640"))
            {
                _mobileNumber = _mobileNumber.Remove(2, 1);
            }

            if (!_mobileNumber.StartsWith(telInput))
            {
                _mobileNumber = telInput + _mobileNumber;
            }

            // check if it's the right length
            if (_mobileNumber.Length != 12)
            {
                return _mobileNumber;
            }

            return _mobileNumber;
        }

        private static string CleanNumber(string phone)
        {
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(phone, "");
        }

        public static long CalulateSegment(string s)
        {
            Encoding u16LE = Encoding.Unicode;
            char[] charArr = s.ToCharArray();
            int countstr = charArr.Count();
            long cal = 0;
            Encoding u7 = Encoding.UTF7;
            cal = u7.GetByteCount(charArr, 0, countstr);

            int headSendBits = 0;
            bool hasVietNameese;
            //true
            hasVietNameese = ContainsUnicodeCharacter(s);
            if (!hasVietNameese)
            {
                cal = countstr;
                cal = (cal * 7);
                headSendBits = 16;
            }
            else
            {
                cal = u16LE.GetByteCount(charArr, 0, countstr);
                cal = (cal * 7) + (countstr * 2);
                headSendBits = 48;
            }
            long realCal = (Convert.ToInt64(cal) / 1120);
            realCal = realCal + 1;
            if (realCal > 1)
            {
                for (int i = 0; i < realCal; i++)
                {
                    cal = cal + headSendBits;
                    realCal = (Convert.ToInt64(cal) / 1120);
                    if (i == 0)
                    {
                        realCal += 1;
                    }

                }
                realCal = (Convert.ToInt64(cal) / 1120);
                if (Convert.ToInt64(cal) % 1120 != 0)
                {
                    realCal = realCal + 1;
                }
            }
            return realCal;
        }

        public static string ToFullSearch(this string searchText)
        {
            return Regex.Replace(searchText, "[^a-zA-Z0-9_.@áàạảãâấầậẩẫăắằặẳẵÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴéèẹẻẽêếềệểễÉÈẸẺẼÊẾỀỆỂỄóòọỏõôốồộổỗơớờợởỡÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠúùụủũưứừựửữÚÙỤỦŨƯỨỪỰỬỮíìịỉĩÍÌỊỈĨđĐýỳỵỷỹÝỲỴỶỸ]+", " ", RegexOptions.Compiled);
        }

        
    }
}

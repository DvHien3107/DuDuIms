using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enrich.Common.Helpers
{
    public static class StringHelper
    {
        public static bool IsLetterOrAddressTitle(this string value)
        {
            var temp = value.ToUpper();

            return temp == "DHR" || temp == "MEVR" || temp == "JUFF" || temp == "M" || temp == "MME" || temp == "MLLE" || temp == "MR" || temp == "MRS" || temp == "MISS"
                || temp == "DHR." || temp == "MEVR." || temp == "JUFF." || temp == "M." || temp == "MME." || temp == "MLLE." || temp == "MR." || temp == "MRS."
                || temp == "MISS.";
        }

        public static bool IsMiddleName(this string value)
        {
            var temp = value.ToUpper();
            return temp == "DE" || temp == "DU" || temp == "VAN";
        }

        public static bool IsNotEmpty(this string source) => !string.IsNullOrWhiteSpace(source);    

        public static string RemoveEmptyPhonesOrEmails(this string listPhoneOrEmail, bool distinct = true)
        {
            if (string.IsNullOrWhiteSpace(listPhoneOrEmail))
            {
                return string.Empty;
            }

            return listPhoneOrEmail.SplitEx(distinct: distinct).ToStringList(", ").Trim(new[] { ' ', ',' });
        }

        public static string[] SplitEx(this string source, bool removeEmpty = true, bool distinct = false)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return new string[] { };
            }

            var splits = source.Split(Constants.SEPARATOR_CHARACTERS, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
            return distinct ? splits.Distinct().ToArray() : splits;
        }

        public static int[] ToListInt(this string source)
         => source.SplitEx().Select(a => a.GetInt()).ToArray();

        public static string[] ToListString(this string source)
        => source.SplitEx().Select(a => a.GetString()).ToArray();

        //refer: CodeHelper.MakeValidFileName
        public static string NormalizeFileName(this string original, bool checkLength, int maxLength = 70)
        {
            var fileName = Regex.Replace(original, @"[^a-z|A-Z|0-9| |.|,|_|-]", "_").RemoveNoneASCII().Trim();

            if (checkLength && maxLength > 0)
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                if (name.Length > maxLength)
                {
                    name = name.Left(maxLength);
                    fileName = Path.ChangeExtension(name, Path.GetExtension(fileName));
                }
            }

            return fileName.Trim();
        }

        public static string RemoveNoneASCII(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return source;

            var sb = new StringBuilder(source.Length);
            foreach (char c in source)
            {
                if ((int)c > 127) // you probably don't want 127 either
                    sb.Append('_');
                else
                if ((int)c < 32)  // I bet you don't want control characters 
                    sb.Append('_');
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
      
        public static string FixHtml(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            return source
                .Replace(Convert.ToChar(12), Convert.ToChar(32))
                .Replace("<p>&nbsp;</p>", "<br/>");
        }

        /// <summary>
        /// Convert only "\n" to "\r\n"
        /// </summary>
        public static string FixNewLines(this string source)
        {
            if (source?.IndexOf("\n") >= 0)
            {
                return source
                    .Replace("\r\n", "#__NEW_LINE__#")
                    .Replace("\n", Environment.NewLine)
                    .Replace("#__NEW_LINE__#", "\r\n");
            }

            return source;
        }
               
        public static string Left(this string source, int left)
        {
            if (string.IsNullOrWhiteSpace(source) || left <= 0)
            {
                return source;
            }

            return source.Length > left ? source.Substring(0, left) : source;
        }

        public static string RemoveSpecialCharacterFromEmailAddress(string email)
        {
            if (string.IsNullOrEmpty(email)) return email;
            return email.Trim('\'', '[', ']', '\"');
        }

        /// <summary>
        /// Remove special character
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveSpecialCharacter(string source)
        {
            if (string.IsNullOrEmpty(source)) return source;
            return Regex.Replace(source, @"[^0-9a-zA-Z\._]", string.Empty);
        }

        public static string EncodeSql(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Replace("'", "''");
            }
            return string.Empty;
        }

        public static string SubStringByRangeKey(this string source, string startKey, string endKey, string[] extendEndKeys = null)
        {
            var startIndex = !string.IsNullOrWhiteSpace(startKey) ? source.IndexOf(startKey, StringComparison.OrdinalIgnoreCase) : 0;
            if (startIndex < 0)
            {
                return string.Empty;
            }

            var endIndex = source.IndexOf(endKey, StringComparison.OrdinalIgnoreCase);
            if (endIndex < 0 && extendEndKeys?.Length > 0)
            {
                var extendStartIndex = Array.FindIndex(extendEndKeys, x => x.Equals(startKey, StringComparison.OrdinalIgnoreCase));
                for (var index = extendStartIndex + 1; index < extendEndKeys.Length; index++)
                {
                    endIndex = source.IndexOf(extendEndKeys[index], StringComparison.OrdinalIgnoreCase);
                    if (endIndex >= 0)
                    {
                        break;
                    }
                }
                if (endIndex < 0)
                {
                    return string.Empty;
                }
            }

            startIndex += startKey.Length;

            if (startIndex == endIndex)
            {
                return string.Empty;
            }

            var feedback = endIndex > 0 ? source.Substring(startIndex, endIndex - startIndex) : source.Substring(startIndex);

            return feedback.Trim();
        }

        public static string ToFriendlyCase(this string value)
            => !string.IsNullOrEmpty(value) ? Regex.Replace(value, "(?!^)([A-Z])", " $1") : string.Empty;

        public static bool EqualsEx(this string source, string value, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
          => source != null && value != null && source.Equals(value, comparison);

        public static bool StartsWithEx(this string source, string value, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            => source != null && value != null && source.StartsWith(value, comparison);

        public static bool EndsWithEx(this string source, string value, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            => source != null && value != null && source.EndsWith(value, comparison);

        public static bool ContainsEx(this string source, string value, bool ignoreCase = true)
            => source != null && value != null && (ignoreCase ? source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0 : source.Contains(value));
             
        public static string ReplaceSpecialChars(this string source, params (char OldValue, char NewValue)[] replacements)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            var replaceString = source;

            foreach (var item in replacements)
            {
                replaceString = replaceString.Replace(item.OldValue, item.NewValue);
            }

            return replaceString;
        }

        public static string ReplaceSpecialChars(this string source, params (string OldValue, string NewValue)[] replacements)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            var replaceString = source;

            foreach (var item in replacements)
            {
                replaceString = replaceString.Replace(item.OldValue, item.NewValue);
            }

            return replaceString;
        }

        /// <summary>
        /// Check and return valid VAT number
        /// </summary>
        /// <param name="inputVATNumber"></param>
        /// <param name="validVATNumber"></param>
        /// <param name="fullVATNumber"></param>
        /// <returns></returns>
        public static bool TryGetValidVATNumber(string inputVATNumber, out string validVATNumber, bool fullVATNumber = false)
        {
            bool isValid = false;
            validVATNumber = string.Empty;

            if (!string.IsNullOrWhiteSpace(inputVATNumber))
            {
                // Remove spaces and dots
                inputVATNumber = inputVATNumber.Replace(" ", "");
                inputVATNumber = inputVATNumber.Replace(".", "");

                // Define patterns : only contain 10 digits, start with BE and only contain 10 digits
                Regex regexTenDigits = new Regex(@"^[0-9]{10}$");
                Regex regexBEAndTenDigits = new Regex(@"^BE[0-9]{10}$");

                // Check input VAT number
                if (regexTenDigits.Match(inputVATNumber).Success)
                {
                    validVATNumber = string.Format("BE{0}", inputVATNumber);
                    isValid = true;
                }
                else if (regexBEAndTenDigits.Match(inputVATNumber).Success)
                {
                    validVATNumber = inputVATNumber;
                    isValid = true;
                }

                if (!fullVATNumber)
                    validVATNumber = validVATNumber.Replace("BE", string.Empty);
            }

            return isValid;
        }


        public static bool IsValidEmail(this string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string ParameterForSql(string input) => string.IsNullOrEmpty(input) ? "NULL" : $"'{input}'";
        public static string ParameterForSql(bool input) => input == true ? "1" : "0";
        public static string ParameterForSql(DateTime? input) => input.HasValue ? $"'{input.Value.ToString("yyyy-MM-dd hh:mm:ss.fff")}'" : "NULL";
        public static string ParameterForSql(int? input) => input != null ? input.Value.ToString() : "NULL";
    }
}

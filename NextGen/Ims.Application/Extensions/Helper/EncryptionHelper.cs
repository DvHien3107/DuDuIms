using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Extensions.Helper
{
    public static class EncryptionHelper
    {
        #region Client
        public static string strLine1, strLine2, strLine3, strLine4, strLine5, strLine6;
        public static string RCPLine1, RCPLine2, RCPLine3, RCPLine4;
        public static void ReadFile()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"DLL\\DNInfo.dll";
            StreamReader sr = new StreamReader(path);
            string input;
            int i = 1;
            while ((input = sr.ReadLine()) != null)
            {
                switch (i)
                {
                    case 1:
                        strLine1 = input;
                        i++;
                        break;
                    case 2:
                        strLine2 = input;
                        i++;
                        break;
                    case 3:
                        strLine3 = input;
                        i++;
                        break;
                    case 4:
                        strLine4 = input;
                        i++;
                        break;
                    case 5:
                        strLine5 = input;
                        i++;
                        break;
                    case 6:
                        strLine6 = input;
                        i++;
                        break;
                }
            }
            sr.Close();
        }

        public static void ReadFileRCP()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"DLL\\DNRCP.dll";

            StreamReader sr = new StreamReader(path);
            string input;
            int i = 1;
            while ((input = sr.ReadLine()) != null)
            {
                switch (i)
                {
                    case 1:
                        RCPLine1 = input;
                        i++;
                        break;
                    case 2:
                        RCPLine2 = input;
                        i++;
                        break;
                    case 3:
                        RCPLine3 = input;
                        i++;
                        break;
                    case 4:
                        RCPLine4 = input;
                        i++;
                        break;
                }
            }
            sr.Close();
        }
        public static void WriteFile(string Server, string Data, string User, string Pass, string Email, string EmailPass)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"DLL\\DNInfo.dll";
            string NewPath = AppDomain.CurrentDomain.BaseDirectory + @"DLL\\dt.txt";
            if (File.Exists(NewPath))
            {
                File.Delete(NewPath);
            }
            File.Copy(path, NewPath);
            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
            streamWriter.WriteLine("DNServer:" + Encrypt(Server));
            streamWriter.WriteLine("DNData:" + Encrypt(Data));
            streamWriter.WriteLine("DNUser:" + Encrypt(User));
            streamWriter.WriteLine("DNPass:" + Encrypt(Pass));
            streamWriter.WriteLine("DNEmail:" + Encrypt(Email));
            streamWriter.WriteLine("DNPassEmail:" + Encrypt(EmailPass));
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }

        public static void WriteFileRCP(string Server, string Data, string User, string Pass)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"DLL\\DNRCP.dll";
            string NewPath = AppDomain.CurrentDomain.BaseDirectory + @"DLL\\dtrcp.txt";
            if (File.Exists(NewPath))
            {
                File.Delete(NewPath);
            }

            File.Copy(path, NewPath);
            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
            streamWriter.WriteLine("DNServer:" + Encrypt(Server));
            streamWriter.WriteLine("DNData:" + Encrypt(Data));
            streamWriter.WriteLine("DNUser:" + Encrypt(User));
            streamWriter.WriteLine("DNPass:" + Encrypt(Pass));
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }
        #endregion
        public static string Md5Hash(string text)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
            StringBuilder hashSb = new StringBuilder();
            foreach (byte b in hash)
            {
                hashSb.Append(b.ToString("X2"));
            }
            return hashSb.ToString();
        }

        public static string Encrypt(string Text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("DNSoft@gmail.com");
            byte[] bytes2 = Encoding.UTF8.GetBytes(Text);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes("@DNSolution.com", null);
            byte[] bytes3 = passwordDeriveBytes.GetBytes(32);
            ICryptoTransform transform = new RijndaelManaged
            {
                Mode = CipherMode.CBC
            }.CreateEncryptor(bytes3, bytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            cryptoStream.Write(bytes2, 0, bytes2.Length);
            cryptoStream.FlushFinalBlock();
            byte[] inArray = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(inArray);
        }
        public static string Decrypt(string EncryptedText)
        {
            byte[] bytes = Encoding.ASCII.GetBytes("DNSoft@gmail.com");
            byte[] array = Convert.FromBase64String(EncryptedText);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes("@DNSolution.com", null);
            byte[] bytes2 = passwordDeriveBytes.GetBytes(32);
            ICryptoTransform transform = new RijndaelManaged
            {
                Mode = CipherMode.CBC
            }.CreateDecryptor(bytes2, bytes);
            MemoryStream memoryStream = new MemoryStream(array);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            byte[] array2 = new byte[array.Length];
            int count = cryptoStream.Read(array2, 0, array2.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(array2, 0, count);
        }
    }
}

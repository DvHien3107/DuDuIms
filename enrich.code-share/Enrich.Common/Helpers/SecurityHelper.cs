using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Enrich.Common.Helpers
{
    public static class SecurityHelper
    {
        //enrich@2019
        private static string _defaultPass = Constants.KeyCode;

        public static String Md5Encrypt(String strEncrypt)
        {
            MD5 md5Service = MD5.Create();
            byte[] data = Encoding.ASCII.GetBytes(strEncrypt);
            data = md5Service.ComputeHash(data);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        #region Generate salt and key
        private static string GenerateSaltKey(string password)
        {
            Rfc2898DeriveBytes rfc2898db = new Rfc2898DeriveBytes(password, 16, 10000);

            byte[] data = new byte[48];
            Buffer.BlockCopy(rfc2898db.Salt, 0, data, 0, 16);
            Buffer.BlockCopy(rfc2898db.GetBytes(32), 0, data, 16, 32);
            return Convert.ToBase64String(data);
        }
        private static byte[] GenerateKey(string password, byte[] salt)
        {
            Rfc2898DeriveBytes rfc2898db = new Rfc2898DeriveBytes(password, salt, 10000);
            return rfc2898db.GetBytes(32);
        }
        #endregion

        public static string Encrypt(string plain, string keycode = "")
        {
            try
            {
                if (plain == null || plain.Length == 0) return null;
                var secretkey = string.IsNullOrEmpty(keycode) ? _defaultPass : keycode;
                byte[] encrypted;
                byte[] data = Encoding.UTF8.GetBytes(plain);
                string saltKeyStr = GenerateSaltKey(secretkey);
                byte[] saltKeyB = Convert.FromBase64String(saltKeyStr);
                byte[] salt = new byte[16];
                byte[] key = new byte[32];
                Buffer.BlockCopy(saltKeyB, 0, salt, 0, 16);
                Buffer.BlockCopy(saltKeyB, 16, key, 0, 32);
                saltKeyStr = null;
                saltKeyB = null;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (AesCryptoServiceProvider aes256 = new AesCryptoServiceProvider())
                    {
                        aes256.KeySize = 256;
                        aes256.BlockSize = 128;
                        aes256.GenerateIV();
                        aes256.Padding = PaddingMode.PKCS7;
                        aes256.Mode = CipherMode.CBC;
                        aes256.Key = key;
                        key = null;

                        using (CryptoStream cs = new CryptoStream(ms, aes256.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            ms.Write(aes256.IV, 0, aes256.IV.Length);
                            ms.Write(salt, 0, 16);
                            cs.Write(data, 0, plain.Length);
                        }
                    }
                    encrypted = ms.ToArray();
                }
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Decrypt(string cipher, string keycode = "")
        {
            try
            {
                if (cipher == null || cipher.Length == 0) return null;
                var secretkey = string.IsNullOrEmpty(keycode) ? _defaultPass : keycode;
                byte[] decrypted;
                //byte[] data = Convert.FromBase64String(cipher);
                byte[] data = Convert.FromBase64String(cipher);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    using var aes256 = new AesCryptoServiceProvider();
                    byte[] iv = new byte[16];
                    byte[] salt = new byte[16];
                    ms.Read(iv, 0, 16);
                    ms.Read(salt, 0, 16);

                    aes256.KeySize = 256;
                    aes256.BlockSize = 128;
                    aes256.IV = iv;
                    aes256.Padding = PaddingMode.PKCS7;
                    aes256.Mode = CipherMode.CBC;
                    aes256.Key = GenerateKey(secretkey, salt);

                    using (var cs = new CryptoStream(ms, aes256.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        byte[] temp = new byte[ms.Length - 16 - 16 + 1];
                        decrypted = new byte[cs.Read(temp, 0, temp.Length)];
                        Buffer.BlockCopy(temp, 0, decrypted, 0, decrypted.Length);
                    }
                }
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string EncryptString(string plainText, string key = "")
        {
            key = Md5Encrypt(string.IsNullOrEmpty(key) ? _defaultPass : key);
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

        public static string DecryptString(string cipherText, string key = "")
        {
            key = Md5Encrypt(string.IsNullOrEmpty(key) ? _defaultPass : key);
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
    }
}
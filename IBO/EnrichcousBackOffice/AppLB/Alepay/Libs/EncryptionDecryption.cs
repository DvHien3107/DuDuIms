using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.AppLB.Alepay.Libs
{
    public class EncryptionDecryption
    {
        static RsaKeyParameters rsaKeyParameters;
        static string publicKey = ConfigurationManager.AppSettings["EncryptKey_Alepay"];
        static EncryptionDecryption()
        {
            var keyInfoData = Convert.FromBase64String(publicKey);
            rsaKeyParameters = PublicKeyFactory.CreateKey(keyInfoData) as RsaKeyParameters;
        }

        public static string RSAEncrypt(object obj)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new TickDateTimeConverter() }
            };

            var serialized = JsonConvert.SerializeObject(obj, settings);
            var payloadBytes = Encoding.UTF8.GetBytes(serialized);

            var cipher = GetAsymmetricBlockCipher(true);
            var encrypted = Process(cipher, payloadBytes);

            var encoded = Convert.ToBase64String(encrypted);
            return encoded;
        }

        public static T RSADecrypt<T>(string encryptedText)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new TickDateTimeConverter() }
            };

            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);

            var cipher = GetAsymmetricBlockCipher(false);
            var decrypted = Process(cipher, cipherTextBytes);

            var decoded = Encoding.UTF8.GetString(decrypted);

            return JsonConvert.DeserializeObject<T>(decoded, settings);
        }

        private static IAsymmetricBlockCipher GetAsymmetricBlockCipher(bool forEncryption)
        {
            var cipher = new Pkcs1Encoding(new RsaEngine());
            cipher.Init(forEncryption, rsaKeyParameters);

            return cipher;
        }

        private static byte[] Process(IAsymmetricBlockCipher cipher, byte[] payloadBytes)
        {
            int length = payloadBytes.Length;
            int blockSize = cipher.GetInputBlockSize();

            var plainTextBytes = new List<byte>();
            for (int chunkPosition = 0; chunkPosition < length; chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, length - chunkPosition);
                plainTextBytes.AddRange(cipher.ProcessBlock(
                    payloadBytes, chunkPosition, chunkSize
                ));
            }

            return plainTextBytes.ToArray();
        }

        public static string MD5Encrypt(string input)
        {
            byte[] asciiBytes = ASCIIEncoding.ASCII.GetBytes(input);
            byte[] hashedBytes = MD5CryptoServiceProvider.Create().ComputeHash(asciiBytes);
            string hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hashedString;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DrFee.Utils
{
    public class CustomDES
    {
        private static readonly byte[] key = new byte[] { 80, 137, 33, 204, 248, 160, 176, 168 };
        private static readonly byte[] iv = new byte[] { 126, 227, 93, 176, 76, 31, 52, 170 };

        public static string Encrypt(string originalString)
        {
            if (string.IsNullOrEmpty(originalString))
            {
                throw new ArgumentNullException
                       ("The string which needs to be encrypted can not be null.");
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                cryptoProvider.CreateEncryptor(key, iv),
                CryptoStreamMode.Write
            );
            StreamWriter writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();
            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        public static string Decrypt(string cryptedString)
        {
            if (string.IsNullOrEmpty(cryptedString))
                throw new ArgumentNullException("The string which needs to be decrypted can not be null.");
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cryptedString));
            CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                cryptoProvider.CreateDecryptor(key, iv),
                CryptoStreamMode.Read
            );
            StreamReader reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }

        public static string GenerateKey(int number = 100)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, number).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
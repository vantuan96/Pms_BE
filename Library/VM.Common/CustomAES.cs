using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace VM.Common
{
    public class CustomAES
    {
        private static byte[] GetPasswordBytes()
        {
            // The real password characters is stored in System.SecureString
            // Below code demonstrates on converting System.SecureString into Byte[]
            // Credit: http://social.msdn.microsoft.com/Forums/vstudio/en-US/f6710354-32e3-4486-b866-e102bb495f86/converting-a-securestring-object-to-byte-array-in-net

            byte[] ba = null;

            var s = new System.Security.SecureString();
            s.AppendChar('w');
            s.AppendChar('e');
            s.AppendChar('l');
            s.AppendChar('c');
            s.AppendChar('o');
            s.AppendChar('m');
            s.AppendChar('e');
            s.AppendChar('v');
            s.AppendChar('i');
            s.AppendChar('n');
            // Convert System.SecureString to Pointer
            IntPtr unmanagedBytes = Marshal.SecureStringToGlobalAllocAnsi(s);
            try
            {
                // You have to mark your application to allow unsafe code
                // Enable it at Project's Properties > Build
                unsafe
                {
                    byte* byteArray = (byte*)unmanagedBytes.ToPointer();

                    // Find the end of the string
                    byte* pEnd = byteArray;
                    while (*pEnd++ != 0) { }
                    // Length is effectively the difference here (note we're 1 past end) 
                    int length = (int)((pEnd - byteArray) - 1);

                    ba = new byte[length];

                    for (int i = 0; i < length; ++i)
                    {
                        // Work with data in byte array as necessary, via pointers, here
                        byte dataAtIndex = *(byteArray + i);
                        ba[i] = dataAtIndex;
                    }
                }
            }
            finally
            {
                // This will completely remove the data from memory
                Marshal.ZeroFreeGlobalAllocAnsi(unmanagedBytes);
            }
            return System.Security.Cryptography.SHA256.Create().ComputeHash(ba);
        }

        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            byte[] saltBytes = passwordBytes;
            // Example:
            //saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;
            // Set your salt here to meet your flavor:
            byte[] saltBytes = passwordBytes;
            // Example:
            //saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        public static string Encrypt(string text)
        {
            byte[] originalBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = null;

            // Hash the password with SHA256
            var passwordBytes = SHA256.Create().ComputeHash(GetPasswordBytes());

            // Getting the salt size
            int saltSize = GetSaltSize(passwordBytes);
            // Generating salt bytes
            byte[] saltBytes = GetRandomBytes(saltSize);

            // Appending salt bytes to original bytes
            byte[] bytesToBeEncrypted = new byte[saltBytes.Length + originalBytes.Length];
            for (int i = 0; i < saltBytes.Length; i++)
            {
                bytesToBeEncrypted[i] = saltBytes[i];
            }
            for (int i = 0; i < originalBytes.Length; i++)
            {
                bytesToBeEncrypted[i + saltBytes.Length] = originalBytes[i];
            }

            encryptedBytes = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string decryptedText)
        {
            byte[] bytesToBeDecrypted = Convert.FromBase64String(decryptedText);

            // Hash the password with SHA256
            var passwordBytes = SHA256.Create().ComputeHash(GetPasswordBytes());

            byte[] decryptedBytes = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            // Getting the size of salt
            int saltSize = GetSaltSize(passwordBytes);

            // Removing salt bytes, retrieving original bytes
            byte[] originalBytes = new byte[decryptedBytes.Length - saltSize];
            for (int i = saltSize; i < decryptedBytes.Length; i++)
            {
                originalBytes[i - saltSize] = decryptedBytes[i];
            }

            return Encoding.UTF8.GetString(originalBytes);
        }

        public static int GetSaltSize(byte[] passwordBytes)
        {
            var key = new Rfc2898DeriveBytes(passwordBytes, passwordBytes, 1000);
            byte[] ba = key.GetBytes(2);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ba.Length; i++)
            {
                sb.Append(Convert.ToInt32(ba[i]).ToString());
            }
            int saltSize = 0;
            string s = sb.ToString();
            foreach (char c in s)
            {
                int intc = Convert.ToInt32(c.ToString());
                saltSize = saltSize + intc;
            }

            return saltSize;
        }

        public static byte[] GetRandomBytes(int length)
        {
            byte[] ba = new byte[length];
            RNGCryptoServiceProvider.Create().GetBytes(ba);
            return ba;
        }
    }
}

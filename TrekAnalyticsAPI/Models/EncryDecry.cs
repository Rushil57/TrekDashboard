using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AnalyticsAPI.Models
{
    public class EncryDecry
    {
        internal string Encrypt(string Data)
        {
            try
            {

                string passPhrase = ConfigurationManager.AppSettings["passPhrase"];
                string saltValue = ConfigurationManager.AppSettings["saltValue"];
                string hashAlgorithm = ConfigurationManager.AppSettings["hashAlgorithm"];
                int passwordIterations = Convert.ToInt16(ConfigurationManager.AppSettings["passwordIterations"]);
                string initVector = ConfigurationManager.AppSettings["initVector"];
                int keySize = Convert.ToInt16(ConfigurationManager.AppSettings["keySize"]);

                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(Data);

                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);
                byte[] keyBytes = password.GetBytes(keySize / 8);

                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();

                byte[] cipherTextBytes = memoryStream.ToArray();

                memoryStream.Close();
                cryptoStream.Close();

                string cipherText = Convert.ToBase64String(cipherTextBytes);

                return cipherText;

            }
            catch { }

            return "";
        }
        internal string Decrypt(string Data)
        {
            try
            {

                string passPhrase = ConfigurationManager.AppSettings["passPhrase"];
                string saltValue = ConfigurationManager.AppSettings["saltValue"];
                string hashAlgorithm = ConfigurationManager.AppSettings["hashAlgorithm"];
                int passwordIterations = Convert.ToInt16(ConfigurationManager.AppSettings["passwordIterations"]);
                string initVector = ConfigurationManager.AppSettings["initVector"];
                int keySize = Convert.ToInt16(ConfigurationManager.AppSettings["keySize"]);

                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
                byte[] cipherTextBytes = Convert.FromBase64String(Data);

                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);
                byte[] keyBytes = password.GetBytes(keySize / 8);

                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

                byte[] plainTextBytes = new byte[cipherTextBytes.Length];

                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                memoryStream.Close();
                cryptoStream.Close();

                string plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

                return plainText;

            }
            catch { }

            return "";
        }


        internal class UserInfo
        {
            public int UserID { get; set; }
            public int UserLevel { get; set; }
            public int CallerID { get; set; }
            public string LoadedPreviously { get; set; }
            public string FirstUser { get; set; }
        }


        internal class AccountInfo
        {
            public string AccountID { get; set; }
            public int UserID { get; set; }
            public int UserLevel { get; set; }
            public int CallerID { get; set; }
            public string FirstUser { get; set; }
        }
    }
}
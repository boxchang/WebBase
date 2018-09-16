using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.IO;

namespace BoxCommonLib
{
    class Register
    {
        public string GetRegister(string encryptedData, string saltkey)
        {
            return Encrypter.DecryptAES(encryptedData, saltkey);
        }
    }

    public static class Encrypter
    {
        // Fields
        private const string C_F_KEY = "JgeTriRMyeHsuaNG";

        // Methods
        public static string DecryptAES(string encryptedData, string saltkey)
        {
            byte[] salt = Encoding.UTF8.GetBytes(saltkey.Substring(0, 9));
            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes("JgeTriRMyeHsuaNG", salt);
            RijndaelManaged managed = new RijndaelManaged();
            managed.KeySize = 0x80;
            managed.Key = bytes.GetBytes(0x10);
            managed.BlockSize = 0x80;
            managed.IV = bytes.GetBytes(0x10);
            ICryptoTransform transform = managed.CreateDecryptor();
            byte[] buffer = Convert.FromBase64String(encryptedData);
            MemoryStream stream = new MemoryStream();
            CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write);
            stream2.Write(buffer, 0, buffer.Length);
            stream2.Flush();
            stream2.Close();
            byte[] buffer3 = stream.ToArray();
            return Encoding.UTF8.GetString(buffer3);
        }

        public static EncryptData EncryptAES(string input, int saltLength)
        {
            EncryptData data = new EncryptData();
            Dns.GetHostName();
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            string rendon = GetRendon(saltLength);
            byte[] salt = Encoding.UTF8.GetBytes(rendon.Substring(0, 9));
            data.SaltCode = rendon.ToString();
            RijndaelManaged managed = new RijndaelManaged();
            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes("JgeTriRMyeHsuaNG", salt);
            managed.KeySize = 0x80;
            managed.Key = bytes.GetBytes(0x10);
            managed.BlockSize = 0x80;
            managed.IV = bytes.GetBytes(0x10);
            ICryptoTransform transform = managed.CreateEncryptor();
            MemoryStream stream = new MemoryStream();
            CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write);
            stream2.Write(buffer, 0, buffer.Length);
            stream2.FlushFinalBlock();
            stream2.Close();
            string str2 = Convert.ToBase64String(stream.ToArray());
            data.ConfirmationCode = str2;
            return data;
        }

        private static string GetRendon(int length)
        {
            StringBuilder builder = new StringBuilder();
            char[] chArray = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            int num = RNG.Next(length, length);
            for (int i = 0; i < num; i++)
            {
                builder.Append(chArray[RNG.Next(chArray.Length - 1)]);
            }
            return builder.ToString();
        }
    }

    internal static class RNG
    {
        // Fields
        private static byte[] rb = new byte[4];
        private static RNGCryptoServiceProvider rngp = new RNGCryptoServiceProvider();

        // Methods
        internal static int Next()
        {
            rngp.GetBytes(rb);
            int num = BitConverter.ToInt32(rb, 0);
            if (num < 0)
            {
                num = -num;
            }
            return num;
        }

        internal static int Next(int max)
        {
            rngp.GetBytes(rb);
            int num = BitConverter.ToInt32(rb, 0) % (max + 1);
            if (num < 0)
            {
                num = -num;
            }
            return num;
        }

        internal static int Next(int min, int max)
        {
            return (Next(max - min) + min);
        }
    }

    public class EncryptData
    {
        // Fields
        private string confirmation = string.Empty;
        private string salt = string.Empty;

        // Properties
        public string ConfirmationCode
        {
            get
            {
                return this.confirmation;
            }
            set
            {
                this.confirmation = value;
            }
        }

        public string SaltCode
        {
            get
            {
                return this.salt;
            }
            set
            {
                this.salt = value;
            }
        }
    }
}

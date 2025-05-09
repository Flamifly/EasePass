﻿/*
MIT License

Copyright (c) 2023 Julius Kirsch

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
*/

using EasePass.Extensions;
using EasePass.Settings;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace EasePass.Helper
{
    internal class EncryptDecryptHelper
    {
        public static byte[] EncryptStringAES(string plainText, string password, string salt)
        {
            SecureString pw = new SecureString();
            for (int i = 0; i < password.Length; i++)
            {
                pw.AppendChar(password[i]);
            }

            return EncryptStringAES(plainText, pw, salt);
        }

        public static (string decryptedString, bool correctPassword) DecryptStringAES(byte[] cipherText, string password, string salt)
        {
            return DecryptStringAES(cipherText, password.ConvertToSecureString(), salt);
        }


        public static byte[] EncryptStringAES(string plainText, SecureString password, string salt = "")
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = GetCryptionKey(password, salt);
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt, Encoding.UTF8))
                    {
                        swEncrypt.Write(plainText);
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        public static (string decryptedString, bool correctPassword) DecryptStringAES(byte[] cipherText, SecureString password, string salt = "")
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = GetCryptionKey(password, salt);
                aesAlg.IV = cipherText.Take(16).ToArray();

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(cipherText.Skip(16).ToArray()))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                {
                    try
                    {
                        var reader = srDecrypt.ReadToEnd();
                        return (reader, true);
                    }
                    catch (CryptographicException)
                    {
                        return (null, false);
                    }
                }
            }
        }

        private static byte[] ToBytes(SecureString secureString)
        {
            var pUnicodeBytes = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            try
            {
                byte[] unicodeBytes = new byte[secureString.Length * 2];
                byte[] bytes = new byte[unicodeBytes.Length];

                for (var idx = 0; idx < unicodeBytes.Length; ++idx)
                {
                    bytes[idx] = Marshal.ReadByte(pUnicodeBytes, idx);
                }

                return bytes;
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(pUnicodeBytes);
            }
        }

        public static byte[] DeriveEncryptionKey(SecureString password, byte[] salt, int keySizeInBytes, int iterations)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(ToBytes(password), salt, iterations))
            {
                return pbkdf2.GetBytes(keySizeInBytes);
            }
        }

        private static byte[] GetCryptionKey(SecureString pw, string salt = "")
        {
            byte[] saltFromDatabase = Encoding.UTF8.GetBytes(salt.Length == 0 ? SettingsManager.GetSettings(AppSettingsValues.pSalt) : "");
            int keySizeInBytes = 32;
            int iterations = 10000;

            return DeriveEncryptionKey(pw, saltFromDatabase, keySizeInBytes, iterations);
        }
    }
}

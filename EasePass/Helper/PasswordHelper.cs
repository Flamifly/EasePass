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

using EasePass.Models;
using EasePass.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EasePass.Helper
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Specifies if it should be checked if the Password has been leaked
        /// </summary>
        private static bool disableLeakedPasswords = false;
        private const string ABC = "abcdefghijklmnopqrstuvwxyz";
        private const int StringMinRepeat = 5;
        private const int IntegerMinRepeat = 4;

        private static List<CommonPasswordSequence> CommonSequences = new List<CommonPasswordSequence>()
        {
            new CommonPasswordSequence("012345678909876543210", IntegerMinRepeat), // forward and backward
            new CommonPasswordSequence(ABC + ABC, StringMinRepeat), // double abc
            new CommonPasswordSequence("qwertyuiopasdfghjklzxcvbnm", StringMinRepeat), // English keyboard layout
            new CommonPasswordSequence("qwertzuiopasdfghjklyxcvbnm", StringMinRepeat), // German keyboard layout
            new CommonPasswordSequence("hello"), // the following words are based on some stats from Wikipedia
            new CommonPasswordSequence("test"),
            new CommonPasswordSequence("password"),
            new CommonPasswordSequence("service"),
            new CommonPasswordSequence("monkey"),
            new CommonPasswordSequence("letmein"),
            new CommonPasswordSequence("football"),
            new CommonPasswordSequence("baseball"),
            new CommonPasswordSequence("princess"),
            new CommonPasswordSequence("sunshine"),
            new CommonPasswordSequence("iloveyou"),
            new CommonPasswordSequence("admin"),
            new CommonPasswordSequence("starwars"),
            new CommonPasswordSequence("master"),
            new CommonPasswordSequence("lovely"),
            new CommonPasswordSequence("welcome"),
            new CommonPasswordSequence("dragon"),
            new CommonPasswordSequence("superman"),
        };

        public static void Init()
        {
            string[] username = Environment.UserName.Split(" "); // User should not use his username in passwords

            for (int i = 0; i < username.Length; i++)
            {
                if (!int.TryParse(username[i], out int value))
                    CommonSequences.Add(new CommonPasswordSequence(username[i].ToLower()));
            }

            for (int i = 0; i < ABC.Length; i++) // repeating character, i.e. 'aaaaaaa'
            {
                string repeated = new string(ABC[i], StringMinRepeat);
                CommonSequences.Add(new CommonPasswordSequence(repeated, StringMinRepeat));
            }

            for (int i = 0; i < 10; i++) // repeating number, i.e. '5555555'
            {
                string repeated = new string(i.ToString()[0], StringMinRepeat);
                CommonSequences.Add(new CommonPasswordSequence(repeated, IntegerMinRepeat));
            }
        }

        /// <summary>
        /// Generates a new Password
        /// </summary>
        /// <returns>Returns the generated Password</returns>
        public static async Task<string> GeneratePassword()
        {
            disableLeakedPasswords = AppSettings.DisableLeakedPasswords;
            int length = AppSettings.PasswordLength;
            string chars = AppSettings.PasswordChars;
            Random r = new Random();

            StringBuilder password = new StringBuilder();
            do
            {
                password.Clear();
                do
                {
                    if (password.Length > length)
                        password.Clear();

                    password.Append(chars[r.Next(chars.Length)]);
                }
                while (!IsSecure(chars, length, password.ToString()));
            }
            while ((!disableLeakedPasswords) && (await IsPwned(password.ToString()) == true));
            
            return password.ToString();
        }

        /// <summary>
        /// Validates if the given <paramref name="password"/> is Secure
        /// </summary>
        /// <param name="chars">The allowed Characters inside the Password</param>
        /// <param name="length">The Length, which the Password should have</param>
        /// <param name="password">The Password</param>
        /// <returns>Returns <see langword="true"/> if the Password is Secure</returns>
        private static bool IsSecure(ReadOnlySpan<char> chars, int length, ReadOnlySpan<char> password)
        {
            int maxpoints = 2 + GetMaxPoints(chars); // 1 because of length + common sequences
            int securepoints = 0 + GetMaxPoints(password);

            if (password.Length >= length)
            {
                securepoints++;
            }
            if (!ContainsCommonSequences(password))
            {
                securepoints++;
            }
            return securepoints == Math.Min(maxpoints, length);
        }

        /// <summary>
        /// Gets the Amount of Points of the current String
        /// One point is awarded for each criterion met
        /// Criterion: Digit, Lower, Upper, Punction
        /// </summary>
        /// <param name="chars">The String, which will be rated</param>
        /// <returns>Returns the Points of the String.</returns>
        private static int GetMaxPoints(ReadOnlySpan<char> chars)
        {
            int maxpoints = 0;
            bool digit = false;
            bool lower = false;
            bool upper = false;
            bool punctation = false;

            for (int i = 0; i < chars.Length; i++)
            {
                if (!digit && char.IsDigit(chars[i]))
                {
                    digit = true;
                    maxpoints++;
                }
                else if (!lower && char.IsLower(chars[i]))
                {
                    lower = true;
                    maxpoints++;
                }
                else if (!upper && char.IsUpper(chars[i]))
                {
                    upper = true;
                    maxpoints++;
                }
                else if (!punctation && char.IsPunctuation(chars[i]))
                {
                    punctation = true;
                    maxpoints++;
                }
                else if (digit && lower && upper && punctation)
                {
                    break;
                }
            }
            return maxpoints;
        }

        public static bool[] EvaluatePassword(string password)
        {
            bool[] checks = new bool[6];

            checks[0] = password.Any(char.IsLower);
            checks[1] = password.Any(char.IsUpper);
            checks[2] = password.Length >= AppSettings.PasswordLength;
            checks[3] = password.Any(char.IsPunctuation);
            checks[4] = password.Any(char.IsDigit);
            checks[5] = !ContainsCommonSequences(password);

            return checks;
        }

        private static bool ContainsCommonSequences(ReadOnlySpan<char> password)
        {
            int length = CommonSequences.Count;
            for (int i = 0; i < length; i++)
            {
                if (CommonSequences[i].ContainsSequence(password))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the Password has been leaked already
        /// </summary>
        /// <param name="password">The Password which will be checked</param>
        /// <returns>Returns <see langword="true"/> if the Password has been leaked</returns>
        public static async Task<bool?> IsPwned(string password)
        {
            try
            {
                string hash = GetSha1Hash(password);
                string hashPrefix = hash.Substring(0, 5);
                string hashSuffix = hash.Substring(5);

                using (var client = new HttpClient())
                // Checks if the Password has been leaked
                using (var data = await client.GetAsync("https://api.pwnedpasswords.com/range/" + hashPrefix))
                {
                    if (data == null)
                        return null;

                    return ReadIsPwnedData(data, hashSuffix);
                }
            }
            catch
            {
                return null;
            }
        }
        private static bool ReadIsPwnedData(HttpResponseMessage data, string hashSuffix)
        {
            using (var reader = new System.IO.StreamReader(data.Content.ReadAsStream()))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (line == null)
                        continue;

                    // Each line of the returned hash list has the following form "hash:numberOfTimesFound"
                    // The first element is the hash
                    // The second element is the numberOfTimesFound
                    var splitLIne = line.Split(':');

                    var lineHashedSuffix = splitLIne[0];
                    var numberOfTimesPasswordPwned = int.Parse(splitLIne[1]);

                    if (lineHashedSuffix == hashSuffix)
                        return true;
                }
            }
            return false;
        }

        private static string GetSha1Hash(string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha1.ComputeHash(inputBytes);

                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}

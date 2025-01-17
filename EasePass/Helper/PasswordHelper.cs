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
using EasePass.Models;
using EasePass.Settings;
using System;
using System.Collections.Generic;
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

        private static readonly Random random = new Random();

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
            Random r = random;
            disableLeakedPasswords = AppSettings.DisableLeakedPasswords;
            int length = AppSettings.PasswordLength;
            string allowedChars = AppSettings.PasswordChars;
            int alength = allowedChars.Length;
            var aIncludes = GetAllowedIncludes(allowedChars);

            StringBuilder password = new StringBuilder();
            do
            {
                password.Clear();
                for (int i = 0; i < length; i++)
                {
                    password.Append(allowedChars[r.Next(alength)]);
                }
            }
            while ((!IsSecure(password, length, aIncludes)) || ((!disableLeakedPasswords) && (IsPwned(password.ToString()) == true)));

            return password.ToString();
        }

        /// <summary>
        /// Gets the Criterion of the <paramref name="allowedChars"/> which can be met if a Password will be generated by the included chars
        /// </summary>
        /// <param name="allowedChars">The Chars, which are allowed for a Password generation</param>
        /// <returns>Returns every criterion if it will be able to met up the criterion by the <paramref name="allowedChars"/></returns>
        public static (bool includeUpper, bool includeLower, bool includeSpecial, bool includeNumber) GetAllowedIncludes(ReadOnlySpan<char> allowedChars)
        {
            bool includeUpper = false;
            bool includeLower = false;
            bool includeSpecial = false;
            bool includeNumber = false;

            for (int i = 0; i < allowedChars.Length; i++)
            {
                if (!includeUpper && char.IsUpper(allowedChars[i]))
                {
                    includeUpper = true;
                }
                else if (!includeLower && char.IsLower(allowedChars[i]))
                {
                    includeLower = true;
                }
                else if (!includeSpecial && allowedChars[i].IsSpecialChar())
                {
                    includeSpecial = true;
                }
                else if (!includeNumber && char.IsNumber(allowedChars[i]))
                {
                    includeNumber = true;
                }
                else if (includeUpper && includeLower && includeSpecial && includeNumber)
                {
                    break;
                }
            }
            return (includeUpper, includeLower, includeSpecial, includeNumber);
        }

        /// <summary>
        /// Validates if the given <paramref name="password"/> is Secure
        /// </summary>
        /// <param name="password">The Password, which should be validated</param>
        /// <param name="length">The Length, which the Password should have</param>
        /// <param name="includes">Specifies which criterion will be activated/deactivated for the Password validation</param>
        /// <returns>Returns <see langword="true"/> if the Password is Secure</returns>
        /// <remarks>If the any include is set to <see langword="false"/> it won't be checked and the Method will return <see langword="true"/> if every other criteria is included.</remarks>
        public static bool IsSecure(StringBuilder password, int length, (bool includeUpper, bool includeLower, bool includePunction, bool includeNumber) includes)
        {
            return IsSecure(password, length, includes.includeUpper, includes.includeLower, includes.includePunction, includes.includeNumber);
        }

        /// <summary>
        /// Validates if the given <paramref name="password"/> is Secure
        /// </summary>
        /// <param name="password">The Password, which should be validated</param>
        /// <param name="length">The Length, which the Password should have</param>
        /// <param name="includeUpper">Specifies if the Password can include Uppercase letter</param>
        /// <param name="includeLower">Specifies if the Password can include Lowercase letter</param>
        /// <param name="includeSpecial">Specifies if the Password can include Special characters</param>
        /// <param name="includeNumber">Specifies if the Password can include Numeric characters</param>
        /// <returns>Returns <see langword="true"/> if the Password is Secure</returns>
        /// <remarks>If the any include is set to <see langword="false"/> it won't be checked and the Method will return <see langword="true"/> if every other criterion is included.</remarks>
        public static bool IsSecure(StringBuilder password, int length, bool includeUpper = true, bool includeLower = true, bool includeSpecial = true, bool includeNumber = true)
        {
            int pLength = password.Length;
            Span<char> chars = length < 1025 ? stackalloc char[pLength] : new char[pLength];
            password.CopyTo(0, chars, pLength);

            bool upper = !includeUpper;
            bool lower = !includeLower;
            bool special = !includeSpecial;
            bool number = !includeNumber;
            if (length > chars.Length)
                return false;

            if (ContainsCommonSequences(chars))
                return false;

            foreach (char c in chars)
            {
                if (!upper && char.IsUpper(c))
                {
                    upper = true;
                }
                else if (!lower && char.IsLower(c))
                {
                    lower = true;
                }
                else if (!special && c.IsSpecialChar())
                {
                    special = true;
                }
                else if (!number && char.IsNumber(c))
                {
                    number = true;
                }
                else if (upper && lower && special && number)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Evaluates the <paramref name="password"/>
        /// </summary>
        /// <param name="password"></param>
        /// <returns>Returns foreach criteria the bool value if it has meet</returns>
        /// <remarks>
        /// 1. Lowercase
        /// 2. Uppercase
        /// 3. Length
        /// 4. Special char
        /// 5. Number
        /// 6. Common Sequence
        /// </remarks>
        public static bool[] EvaluatePassword(string password)
        {
            bool[] checks = new bool[6];
            var (includeUpper, includeLower, includeSpecial, includeNumber) = GetAllowedIncludes(password);

            checks[0] = includeLower;
            checks[1] = includeUpper;
            checks[2] = password.Length >= AppSettings.PasswordLength;
            checks[3] = includeSpecial;
            checks[4] = includeNumber;
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
        public static bool? IsPwned(string password)
        {
            var res = IsPwnedHelper.IsPwned(password);
            return res == PwnedResult.Error ? null : res == PwnedResult.Leaked;
        }
    }
}
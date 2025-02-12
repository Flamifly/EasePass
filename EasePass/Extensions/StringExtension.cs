/*
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

using Newtonsoft.Json.Linq;
using System;
using System.Security;

namespace EasePass.Extensions;

/// <summary>
/// Includes every Extension for the <see cref="string"/>
/// </summary>
public static class StringExtension
{
    #region Convert
    /// <summary>
    /// Converts the given <paramref name="value"/> to a <see cref="byte"/>[]
    /// </summary>
    /// <param name="value">The Value, which should be converted</param>
    /// <returns>Returns the <paramref name="value"/> as <see cref="byte"/>[]</returns>
    public static byte[] ConvertToBytes(this string value)
    {
        return value.AsSpan().ConvertToBytes();
    }

    /// <summary>
    /// Converts the given <paramref name="plainString"/> to a <see cref="SecureString"/>
    /// </summary>
    /// <param name="plainString">The <see cref="string"/>, which should be converted</param>
    /// <returns>Returns the <paramref name="plainString"/> as <see cref="SecureString"/></returns>
    public static SecureString ConvertToSecureString(this string value)
    {
        SecureString secureString;
        unsafe
        {
            fixed (char* chars = value)
            {
                //create encrypted secure string object
                secureString = new SecureString(chars, value.Length);
                secureString.MakeReadOnly();
                value.ZeroOut();
            }
        }
        return secureString;
    }
    #endregion

    #region ZeroOut
    /// <summary>
    /// Offers existing String type a safe zeroing out method without returning a SecureString object while taking care
    /// not to zero out string literals or values that share the same location as a string literal
    /// Call: myString.SecureClear();
    /// </summary>
    /// <param name="value"></param>
    public static void ZeroOut(this string value)
    {
        object checkInterned = string.IsInterned(value);
        if (checkInterned == null)
        {
            unsafe
            {
                fixed (char* chars = value)
                {
                    //zero out original
                    for (int i = 0; i < value.Length; i++)
                    {
                        chars[i] = '\0';
                    }
                }
            }
        }
    }
    #endregion
}
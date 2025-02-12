using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace EasePass.Extensions
{
    /// <summary>
    /// Includes every Extension for the <see cref="SecureString"/>
    /// </summary>
    internal static class SecureStringExtension
    {
        #region Constants
        /// <summary>
        /// The Default <see cref="char"/>, which will be used for the <see cref="Trim(char[])"/>
        /// </summary>
        const char DefaultChar = (char)32;
        #endregion

        #region Add
        /// <summary>
        /// Adds the given <paramref name="chars"/> at the end of the <paramref name="secureString"/>
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be extended</param>
        /// <param name="chars">The <see cref="ReadOnlySpan{char}"/>, which should be added to the end of the <paramref name="secureString"/></param>
        /// <returns>Returns the <paramref name="secureString"/>, with the <paramref name="chars"/> at the End.</returns>
        public static SecureString Add(this SecureString secureString, ReadOnlySpan<char> chars) => Add(secureString, chars, false);
        /// <summary>
        /// Adds the given <paramref name="chars"/> at the End/Start of the <paramref name="secureString"/>
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>[], which should be extended</param>
        /// <param name="chars">The <see cref="ReadOnlySpan{char}"/>, which should be added to the End/Start of the <paramref name="secureString"/></param>
        /// <param name="start">Specifies if the Sequence, will be added at the beginning or at the end.</param>
        /// <returns>Returns the <paramref name="secureString"/>, with the <paramref name="chars"/> at the End/Start.</returns>
        public static SecureString Add(this SecureString secureString, ReadOnlySpan<char> chars, bool start)
        {
            switch (start)
            {
                case true:
                    return AddStart(secureString, chars);
                default:
                    return AddEnd(secureString, chars);
            }

        }

        #region AddStart
        /// <summary>
        /// Adds the given <paramref name="chars"/> at the Start of the <paramref name="secureString"/>
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be extended</param>
        /// <param name="chars">The <see cref="ReadOnlySpan{char}"/>, which should be added to the Start of the <paramref name="secureString"/></param>
        /// <returns>Returns the <paramref name="secureString"/>, with the <paramref name="chars"/> at the Start.</returns>
        public static SecureString AddStart(this SecureString secureString, ReadOnlySpan<char> chars)
        {
            return secureString.ToCharArray().AddStart(chars).ToSecureString();
        }
        #endregion

        #region AddEnd
        /// <summary>
        /// Adds the given <paramref name="chars"/> at the End of the <paramref name="secureString"/>
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be extended</param>
        /// <param name="chars">The <see cref="ReadOnlySpan{char}"/>, which should be added to the End of the <paramref name="secureString"/></param>
        /// <returns>Returns the <paramref name="secureString"/>, with the <paramref name="chars"/> at the End.</returns>
        public static SecureString AddEnd(this SecureString secureString, ReadOnlySpan<char> chars)
        {
            return secureString.ToCharArray().AddEnd(chars).ToSecureString();
        }
        #endregion
        #endregion

        #region IsNullOrEmpty
        /// <summary>
        /// Indicates whether a specified <paramref name="secureString"/> is <see langword="null"/> or empty.
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be checked</param>
        /// <returns>Retrns <see langword="true"/> if the <paramref name="secureString"/> is <see langword="null"/> or empty.</returns>
        public static bool IsNullOrEmpty(this SecureString secureString)
        {
            return (secureString == null || secureString.Length == 0);
        }
        #endregion

        #region StartsWith
        /// <summary>
        /// Determines whether a specified the sequence in <paramref name="secureString"/> appears at the start of the <paramref name="secureString"/>
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which will be checked</param>
        /// <param name="starts">The Sequence, which should appear at the start of <paramref name="secureString"/></param>
        /// <returns>Returns <see langword="true"/> if the <paramref name="secureString"/> starts with the sequence of <paramref name="starts"/>.</returns>
        public static bool StartsWith(this SecureString secureString, ReadOnlySpan<char> starts)
        {
            return ((ReadOnlySpan<char>)secureString.ToCharArray()).StartsWith(starts);
        }
        #endregion

        #region Trim
        /// <summary>
        /// Removes all the leading and trailing occurrences of the <see cref="DefaultChar"/> specified in the <paramref name="secureString"/>.
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be trimmed</param>
        /// <returns>Returns the trimmed Array, without leading and trailing <see cref="DefaultChar"/>.</returns>
        public static SecureString Trim(this SecureString secureString) => Trim(secureString, DefaultChar);
        /// <summary>
        /// Removes all the leading and trailing occurrences of the <paramref name="c"/> specified in the <paramref name="secureString"/>.
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be trimmed</param>
        /// <param name="c">The <see cref="char"/>, which should not lead or trail in the <paramref name="secureString"/></param>
        /// <returns>Returns the trimmed Array, without leading and trailing <paramref name="c"/>.</returns>
        public static SecureString Trim(this SecureString secureString, char c)
        {
            if (secureString.Length == 0)
                return secureString;

            return secureString.ToCharArray().Trim(c).ToSecureString();
        }

        #region TrimEnd
        /// <summary>
        /// Removes all the trailing occurrences of the <see cref="DefaultChar"/> specified in the <paramref name="secureString"/>.
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be trimmed</param>
        /// <returns>Returns the trimmed Array, without trailing <see cref="DefaultChar"/>.</returns>
        public static SecureString TrimEnd(this SecureString secureString) => TrimEnd(secureString, DefaultChar);
        /// <summary>
        /// Removes all the leading occurrences of the <paramref name="c"/> specified in the <paramref name="chars"/>.
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be trimmed</param>
        /// <param name="c">The <see cref="char"/>, which should not trail in the <paramref name="secureString"/></param>
        /// <returns>Returns the trimmed Array, without leading <paramref name="c"/>.</returns>
        public static SecureString TrimEnd(this SecureString secureString, char c)
        {
            if (secureString.Length == 0)
                return secureString;

            return secureString.ToCharArray().TrimEnd(c).ToSecureString();
        }
        #endregion

        #region TrimStart
        /// <summary>
        /// Removes all the leading occurrences of the <see cref="DefaultChar"/> specified in the <paramref name="secureString"/>.
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be trimmed</param>
        /// <returns>Returns the trimmed Array, without leading <see cref="DefaultChar"/>.</returns>
        public static SecureString TrimStart(this SecureString secureString) => TrimStart(secureString, DefaultChar);
        /// <summary>
        /// Removes all the leading occurrences of the <paramref name="c"/> specified in the <paramref name="secureString"/>.
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be trimmed</param>
        /// <param name="c">The <see cref="char"/>, which should not lead in the <paramref name="secureString"/></param>
        /// <returns>Returns the trimmed Array, without leading <paramref name="c"/>.</returns>
        public static SecureString TrimStart(this SecureString secureString, char c)
        {
            if (secureString.Length == 0)
                return secureString;

            return secureString.ToCharArray().TrimStart(c).ToSecureString();
        }
        #endregion
        #endregion

        #region Convert
        /// <summary>
        /// Converts the <paramref name="secureString"/> to a <see cref="string"/>.
        /// This Method should be carefully! By this Method the Secured string will be allocated decrypted!
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be converted</param>
        /// <returns>Returns the <see cref="SecureString"/> as <see cref="string"/>.</returns>
        public static string ConvertToString(this SecureString secureString)
        {
            if (secureString == null || secureString.Length == 0)
            {
                return string.Empty;
            }

            IntPtr intPtr = IntPtr.Zero;
            try
            {
                intPtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(intPtr);
            }
            finally
            {
                if (intPtr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
                }
            }
        }

        /// <summary>
        /// Converts the given <paramref name="secureString"/> to a <see cref="byte"/>[]
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be converted</param>
        /// <returns>Returns the <paramref name="secureString"/> as <see cref="byte"/>[]</returns>
        public static byte[] ToBytes(this SecureString secureString)
        {
            nint pUnicodeBytes = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            try
            {
                byte[] unicodeBytes = new byte[secureString.Length * 2];
                byte[] bytes = new byte[unicodeBytes.Length];

                for (int idx = 0; idx < unicodeBytes.Length; ++idx)
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
        /// <summary>
        /// Converts the given <paramref name="secureString"/> to a <see cref="char"/>[]
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be converted</param>
        /// <returns>Returns the <paramref name="secureString"/> as <see cref="char"/>[]</returns>
        public static char[] ToCharArray(this SecureString secureString)
        {
            IntPtr ptr = Marshal.SecureStringToBSTR(secureString);
            try
            {
                byte b = 1;
                int i = 0;
                List<char> list = new List<char>();

                // Loop through until we hit the string terminator '\0'
                while (((char)b) != '\0')
                {
                    list.Add((char)Marshal.ReadByte(ptr, i));
                    i += 2;
                }
                return list.ToArray();
            }
            catch { }
            finally
            {
                // Free AND ZERO OUT our marshalled BSTR pointer to our secureString
                Marshal.ZeroFreeBSTR(ptr);
            }
            return Array.Empty<char>();
        }

        /// <summary>
        /// Converts the given <paramref name="secureString"/> to a <see cref="Span{char}"/>
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be converted</param>
        /// <returns>Returns the <paramref name="secureString"/> as <see cref="Span{char}"/></returns>
        public static Span<char> AsSpan(this SecureString secureString) => ToCharArray(secureString);

        /// <summary>
        /// Converts the given <paramref name="secureString"/> to a <see cref="ReadOnlySpan{char}"/>
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be converted</param>
        /// <returns>Returns the <paramref name="secureString"/> as <see cref="ReadOnlySpan{char}"/></returns>
        public static ReadOnlySpan<char> AsReadOnlySpan(this SecureString secureString) => ToCharArray(secureString);

        /// <summary>
        /// Converts the given <paramref name="secureString"/> to a <see cref="Span{char}"/>
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be converted</param>
        /// <returns>Returns the <paramref name="secureString"/> as <see cref="Span{char}"/></returns>
        public static Span<char> ToSpan(this SecureString secureString) => ToCharArray(secureString);

        /// <summary>
        /// Converts the given <paramref name="secureString"/> to a <see cref="ReadOnlySpan{char}"/>
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/>, which should be converted</param>
        /// <returns>Returns the <paramref name="secureString"/> as <see cref="ReadOnlySpan{char}"/></returns>
        public static ReadOnlySpan<char> ToReadOnlySpan(this SecureString secureString) => ToCharArray(secureString);
        #endregion
    }
}
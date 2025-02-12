using System;
using System.Security;

namespace EasePass.Extensions
{
    /// <summary>
    /// Includes every Extension for the <see cref="char"/>[]
    /// </summary>
    internal static class CharArrayExtension
    {
        #region Constants
        /// <summary>
        /// The Default <see cref="char"/>, which will be used for the <see cref="Trim(char[])"/>
        /// </summary>
        const char DefaultChar = (char)32;
        #endregion

        #region Add
        /// <summary>
        /// Adds the given <paramref name="chars2"/> at the end of the <paramref name="chars"/>
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be extended</param>
        /// <param name="chars2">The <see cref="ReadOnlySpan{char}"/>, which should be added to the end of the <paramref name="chars"/></param>
        /// <returns>Returns the <paramref name="chars"/>, with the <paramref name="chars2"/> at the End.</returns>
        public static char[] Add(this char[] chars, ReadOnlySpan<char> chars2) => Add(chars, chars2, false);
        /// <summary>
        /// Adds the given <paramref name="chars2"/> at the End/Start of the <paramref name="chars"/>
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be extended</param>
        /// <param name="chars2">The <see cref="ReadOnlySpan{char}"/>, which should be added to the End/Start of the <paramref name="chars"/></param>
        /// <param name="start">Specifies if the Sequence, will be added at the beginning or at the end.</param>
        /// <returns>Returns the <paramref name="chars"/>, with the <paramref name="chars2"/> at the End/Start.</returns>
        public static char[] Add(this char[] chars, ReadOnlySpan<char> chars2, bool start)
        {
            switch (start)
            {
                case true:
                    return AddStart(chars, chars2);
                default:
                    return AddEnd(chars, chars2);
            }

        }

        #region AddStart
        /// <summary>
        /// Adds the given <paramref name="chars2"/> at the Start of the <paramref name="chars"/>
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be extended</param>
        /// <param name="chars2">The <see cref="ReadOnlySpan{char}"/>, which should be added to the Start of the <paramref name="chars"/></param>
        /// <returns>Returns the <paramref name="chars"/>, with the <paramref name="chars2"/> at the Start.</returns>
        public static char[] AddStart(this char[] chars, ReadOnlySpan<char> chars2)
        {
            int i = 0;
            char[] result = new char[chars.Length + chars2.Length];

            FillArrayInternal(result, chars2, ref i);
            FillArrayInternal(result, chars, ref i);

            return result;
        }
        #endregion

        #region AddEnd
        /// <summary>
        /// Adds the given <paramref name="chars2"/> at the End of the <paramref name="chars"/>
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be extended</param>
        /// <param name="chars2">The <see cref="ReadOnlySpan{char}"/>, which should be added to the End of the <paramref name="chars"/></param>
        /// <returns>Returns the <paramref name="chars"/>, with the <paramref name="chars2"/> at the End.</returns>
        public static char[] AddEnd(this char[] chars, ReadOnlySpan<char> chars2)
        {
            int i = 0;
            char[] result = new char[chars.Length + chars2.Length];
            
            FillArrayInternal(result, chars, ref i);
            FillArrayInternal(result, chars2, ref i);

            return result;
        }
        #endregion

        #region FillArrayInternal
        /// <summary>
        /// Fílls the given <paramref name="array"/> with the data in <paramref name="chars"/> beginning at the <paramref name="index"/>
        /// </summary>
        /// <param name="array">The <see cref="char"/>[], which should be filled</param>
        /// <param name="chars">The <see cref="ReadOnlySpan{char}"/>, which contains the data to fill <paramref name="array"/></param>
        /// <param name="index">The Startposition to fill the <paramref name="array"/></param>
        private static void FillArrayInternal(char[] array, ReadOnlySpan<char> chars, ref int index)
        {
            int length = array.Length;
            foreach (char c in chars)
            {
                if (index >= length)
                    break;

                array[index++] = c;
            }
        }
        #endregion

        #endregion

        #region IsNullOrEmpty
        /// <summary>
        /// Indicates whether a specified <paramref name="chars"/> is <see langword="null"/> or empty.
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be checked</param>
        /// <returns>Retrns <see langword="true"/> if the <paramref name="chars"/> is <see langword="null"/> or empty.</returns>
        public static bool IsNullOrEmpty(this char[] chars)
        {
            return (chars == null || chars.Length == 0);
        }
        #endregion

        #region StartsWith
        /// <summary>
        /// Determines whether a specified the sequence in <paramref name="chars"/> appears at the start of the <paramref name="chars"/>
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which will be checked</param>
        /// <param name="starts">The Sequence, which should appear at the start of <paramref name="chars"/></param>
        /// <returns>Returns <see langword="true"/> if the <paramref name="chars"/> starts with the sequence of <paramref name="starts"/>.</returns>
        public static bool StartsWith(this char[] chars, ReadOnlySpan<char> starts)
        {
            return ((ReadOnlySpan<char>)chars).StartsWith(starts);
        }
        #endregion

        #region Convert | ToXXX
        /// <summary>
        /// Converts the given <paramref name="chars"/> to a <see cref="SecureString"/>
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be converted</param>
        /// <returns>Returns the <paramref name="chars"/> as <see cref="SecureString"/></returns>
        public static SecureString ToSecureString(this  char[] chars)
        {
            return ((ReadOnlySpan<char>)chars.AsSpan()).ConvertToSecureString();
        }
        #endregion

        #region Trim
        /// <summary>
        /// Removes all the leading and trailing occurrences of the <see cref="DefaultChar"/> specified in the <paramref name="chars"/>.
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be trimmed</param>
        /// <returns>Returns the trimmed Array, without leading and trailing <see cref="DefaultChar"/>.</returns>
        public static char[] Trim(this char[] chars) => Trim(chars, DefaultChar);
        /// <summary>
        /// Removes all the leading and trailing occurrences of the <paramref name="c"/> specified in the <paramref name="chars"/>.
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be trimmed</param>
        /// <param name="c">The <see cref="char"/>, which should not lead or trail in the <paramref name="chars"/></param>
        /// <returns>Returns the trimmed Array, without leading and trailing <paramref name="c"/>.</returns>
        public static char[] Trim(this char[] chars, char c)
        {
            if (chars.Length == 0)
                return chars;

            int length = chars.Length;
            int occurence = 0;
            int i = length - 1;
            for (; i > 0; i--)
            {
                if (chars[i] == c)
                {
                    occurence++;
                }
                else
                {
                    break;
                }
            }

            if (length == occurence)
                return chars[0..^occurence];

            for (i = 0; i < length; i++)
            {
                if (chars[i] == c)
                    continue;

                break;
            }
            return chars[i..^occurence];
        }

        #region TrimEnd
        /// <summary>
        /// Removes all the trailing occurrences of the <see cref="DefaultChar"/> specified in the <paramref name="chars"/>.
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be trimmed</param>
        /// <returns>Returns the trimmed Array, without trailing <see cref="DefaultChar"/>.</returns>
        public static char[] TrimEnd(this char[] chars) => TrimEnd(chars, DefaultChar);
        /// <summary>
        /// Removes all the leading occurrences of the <paramref name="c"/> specified in the <paramref name="chars"/>.
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be trimmed</param>
        /// <param name="c">The <see cref="char"/>, which should not trail in the <paramref name="chars"/></param>
        /// <returns>Returns the trimmed Array, without leading <paramref name="c"/>.</returns>
        public static char[] TrimEnd(this char[] chars, char c)
        {
            if (chars.Length == 0)
                return chars;

            int occurence = 0;
            for (int i = chars.Length - 1; i > 0; i--)
            {
                if (chars[i] == c)
                {
                    occurence++;
                }
                else
                {
                    break;
                }
            }
            return chars[0..^occurence];
        }
        #endregion

        #region TrimStart
        /// <summary>
        /// Removes all the leading occurrences of the <see cref="DefaultChar"/> specified in the <paramref name="chars"/>.
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be trimmed</param>
        /// <returns>Returns the trimmed Array, without leading <see cref="DefaultChar"/>.</returns>
        public static char[] TrimStart(this char[] chars) => TrimStart(chars, DefaultChar);
        /// <summary>
        /// Removes all the leading occurrences of the <paramref name="c"/> specified in the <paramref name="chars"/>.
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be trimmed</param>
        /// <param name="c">The <see cref="char"/>, which should not lead in the <paramref name="chars"/></param>
        /// <returns>Returns the trimmed Array, without leading <paramref name="c"/>.</returns>
        public static char[] TrimStart(this char[] chars, char c)
        {
            if (chars.Length == 0)
                return chars;

            int i = 0;
            int length = chars.Length;
            for (; i < length; i++)
            {
                if (chars[i] == c)
                    continue;

                break;
            }
            return chars[i..];
        }
        #endregion
        #endregion

        #region ZeroOut
        /// <summary>
        /// Zeros out the given <paramref name="chars"/>
        /// </summary>
        /// <param name="chars">The <see cref="char"/>[], which should be zero out</param>
        /// <returns>Returns the Zero out Array</returns>
        public static char[] ZeroOut(this char[] chars)
        {
            int length = chars.Length;
            for (int i = 0; i < length; i++)
            {
                chars[i] = '\0';
            }
            return chars;
        }
        #endregion
    }
}
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Text;

    /// <summary>
    /// A collection of common string extensions.
    /// </summary>
    public static class StringExtensions
    {
        [ThreadStatic]
        private static StringBuilder _threadStringBuilder;

        /// <summary>
        /// Ensures the replacement builder exists with the given capacity.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <returns>StringBuilder.</returns>
        private static StringBuilder EnsureReplacementBuilder(int capacity)
        {
            var result = _threadStringBuilder;

            if (result == null)
            {
                result = new StringBuilder(capacity);
                _threadStringBuilder = result;
            }
            else
            {
                result.Clear();
                result.EnsureCapacity(capacity);
            }

            return result;
        }

        /// <summary>
        /// Extracts a sub string from the right side.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="length">The length.</param>
        /// <returns>System.String.</returns>
        public static string SubstringRight(this string str, int length)
        {
            return str.Substring(str.Length - length, length);
        }

        /// <summary>
        /// Returns the index of the first alpha character in the string. Returns -1 if no alphabetic characters are found.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>System.Int32.</returns>
        public static int IndexOfFirstAlphaChar(this string text)
        {
            for (var i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the last digit in the string. Returns -1 if no digits are found.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>System.Int32.</returns>
        public static int IndexOfLastDigit(this string text)
        {
            for (var i = text.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(text[i]))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Replaced the old text with the new text regardless of the casing of the letters in the source
        /// and old string values.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>System.String.</returns>
        public static string ReplaceCaseInvariant(this string text, string oldValue, string newValue)
        {
            newValue = newValue ?? string.Empty;
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(oldValue) || oldValue.Equals(newValue, StringComparison.OrdinalIgnoreCase))
                return text;

            var foundAt = 0;
            while ((foundAt = text.IndexOf(oldValue, foundAt, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                text = text.Remove(foundAt, oldValue.Length).Insert(foundAt, newValue);
                foundAt += newValue.Length;
            }

            return text;
        }

        /// <summary>
        /// Checks if two strings are equal in a case-insensitive manner. Utilizes <see cref="StringComparison.OrdinalIgnoreCase" />.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="otherText">The other text.</param>
        /// <returns><c>true</c> if strings are both null or both equal regardless of casing, <c>false</c> otherwise.</returns>
        public static bool EqualsCaseInvariant(this string text, string otherText)
        {
            if (text == null && otherText == null)
                return true;

            return string.Equals(text, otherText, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the given text is contained in the string, regardless of text casing.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="otherText">The other text.</param>
        /// <returns><c>true</c> if the otherText is contained in the string; otherwise, <c>false</c>. Also returns false if either string is null.</returns>
        public static bool ContainsCaseInvariant(this string text, string otherText)
        {
            if (text == null || otherText == null)
                return false;

            return text.IndexOf(otherText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Determines if the string starts with  the supplied string, regardless of casing.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="otherText">The other text.</param>
        /// <returns><c>true</c> if the string starts with the supplied string, <c>false</c> otherwise. Also returns false if either string is null.</returns>
        public static bool StartsWithCaseInvariant(this string text, string otherText)
        {
            if (text == null || otherText == null)
                return false;

            return text.IndexOf(otherText, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Determines if the string ends with the supplied string, regardless of casing.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="otherText">The other text.</param>
        /// <returns><c>true</c> if the string ends with the supplied string, <c>false</c> otherwise. Also returns false if either string is null.</returns>
        public static bool EndsWithCaseInvariant(this string text, string otherText)
        {
            if (text == null || otherText == null)
                return false;

            return text.LastIndexOf(otherText, StringComparison.OrdinalIgnoreCase) == text.Length - otherText.Length;
        }

        /// <summary>
        /// Attempts to replace the last found instance of the given string with a new value.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="oldValue">The old value to replace.</param>
        /// <param name="newValue">The new value to insert.</param>
        /// <returns>System.String.</returns>
        public static string ReplaceLastInstanceOfCaseInvariant(this string text, string oldValue, string newValue)
        {
            newValue = newValue ?? string.Empty;
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(oldValue) || oldValue.Equals(newValue, StringComparison.OrdinalIgnoreCase))
                return text;

            var foundAt = text.LastIndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            if (foundAt >= 0)
            {
                text = text.Remove(foundAt, oldValue.Length).Insert(foundAt, newValue);
            }

            return text;
        }

        /// <summary>
        /// Attempts to replace the first found instance of the given string with a new value.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="oldValue">The old value to replace.</param>
        /// <param name="newValue">The new value to insert.</param>
        /// <returns>System.String.</returns>
        public static string ReplaceFirstInstanceOfCaseInvariant(this string text, string oldValue, string newValue)
        {
            newValue = newValue ?? string.Empty;
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(oldValue) || oldValue.Equals(newValue, StringComparison.OrdinalIgnoreCase))
                return text;

            var foundAt = text.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            if (foundAt >= 0)
            {
                text = text.Remove(foundAt, oldValue.Length).Insert(foundAt, newValue);
            }

            return text;
        }

        /// <summary>
        /// A <see cref="string" /> class extension method that converts the first character of a string, if it is a letter, to its lowercase equivalent.
        /// </summary>
        /// <param name="content">The current instance of the <see cref="string" /> class.</param>
        /// <returns>The content of the provided string, with the first character in lowercase form if it is applicable.</returns>
        public static string FirstCharacterToLowerInvariant(this string content)
        {
            if (string.IsNullOrWhiteSpace(content) || !char.IsLetter(content, 0) || char.IsLower(content, 0))
                return content;

            return char.ToLowerInvariant(content[0]) + content.Substring(1);
        }

        /// <summary>
        /// A <see cref="string" /> class extension method that converts the first character of a string, if it is a letter, to its uppercase equivalent.
        /// </summary>
        /// <param name="content">The current instance of the <see cref="string" /> class.</param>
        /// <returns>The content of the provided string, with the first character in uppercase form if it is applicable.</returns>
        public static string FirstCharacterToUpperInvariant(this string content)
        {
            if (string.IsNullOrWhiteSpace(content) || !char.IsLetter(content, 0) || char.IsUpper(content, 0))
                return content;

            return char.ToUpperInvariant(content[0]) + content.Substring(1);
        }

        /// <summary>
        /// Converts this string into its equivilant format as a json property; having the first character
        /// of the string always be lower case.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>System.String.</returns>
        public static string ToJsonPropertyFormat(this string content)
        {
            return content.FirstCharacterToLowerInvariant();
        }

        /// <summary>
        /// Counts the number of times the phrase appears in the string. This method is case sensitive.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="phrase">The phrase.</param>
        /// <returns>System.Int32.</returns>
        public static int PhraseCount(this string haystack, string phrase)
        {
            if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(phrase))
                return 0;

            return (haystack.Length - haystack.Replace(phrase, string.Empty).Length) / phrase.Length;
        }

        /// <summary>
        /// Replaces all instances of the listed characters with the single replacement character (provided first).
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="replaceWith">The character to replace all other characters (...and in the darkness bind them!).</param>
        /// <param name="charsToReplace">A set of characters to be replaced.</param>
        /// <returns>System.String.</returns>
        public static string ReplaceAll(this string text, char replaceWith, params char[] charsToReplace)
        {
            // Adapted from John Whiter:  https://stackoverflow.com/a/30729572
            if (text == null)
                return null;

            if (charsToReplace == null || charsToReplace.Length == 0)
                return text;

            StringBuilder sb = null;
            for (int i = 0; i < text.Length; i++)
            {
                var temp = text[i];
                var replace = false;

                for (int j = 0; j < charsToReplace.Length; j++)
                {
                    if (temp == charsToReplace[j])
                    {
                        if (sb == null)
                        {
                            sb = EnsureReplacementBuilder(text.Length);
                            if (i > 0)
                                sb.Append(text, 0, i);
                        }

                        replace = true;
                        break;
                    }
                }

                if (replace)
                    sb.Append(replaceWith);
                else
                    sb?.Append(temp);
            }

            return sb == null ? text : sb.ToString();
        }
    }
}
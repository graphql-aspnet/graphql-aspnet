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

    /// <summary>
    /// Helper methods for SpanT.
    /// </summary>
    public static class SpanTExtensions
    {
        /// <summary>
        /// Checks the given span to ensure it is NOT equal to this span. This is a helper method for negating
        /// SequenceEqual for easier readability.
        /// </summary>
        /// <typeparam name="T">The type of item in the spans.</typeparam>
        /// <param name="span">The first span.</param>
        /// <param name="other">The other span to check against.</param>
        /// <returns><c>true</c> if the spans are not equal, <c>false</c> otherwise.</returns>
        public static bool SequenceNotEqual<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other)
            where T : IEquatable<T>
        {
            return !span.SequenceEqual(other);
        }

        /// <summary>
        /// Replaces any instance of the found value with the new value and returns a new span.
        /// </summary>
        /// <typeparam name="T">Type of the span.</typeparam>
        /// <param name="span">The span being updated.</param>
        /// <param name="valuesToReplace">The values to search for in the span and replace.</param>
        /// <param name="newValue">The new value to change to.</param>
        /// <returns>Span&lt;T&gt;.</returns>
        public static Span<T> Replace<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> valuesToReplace, T newValue)
            where T : IEquatable<T>
        {
            var data = new T[span.Length].AsSpan();
            for (var i = 0; i < span.Length; i++)
            {
                if (valuesToReplace.IndexOf(span[i]) >= 0)
                    data[i] = newValue;
                else
                    data[i] = span[i];
            }

            return data;
        }

        /// <summary>
        /// Replaces the specified values in the span with the new provided value.
        /// </summary>
        /// <typeparam name="T">The type of the span being inspected.</typeparam>
        /// <param name="span">The span.</param>
        /// <param name="valuesToReplace">The values to replace.</param>
        /// <param name="newValue">The new value.</param>
        public static void Replace<T>(this Span<T> span, ReadOnlySpan<T> valuesToReplace, T newValue)
            where T : IEquatable<T>
        {
            for (var i = 0; i < span.Length; i++)
            {
                if (valuesToReplace.IndexOf(span[i]) >= 0)
                    span[i] = newValue;
            }
        }

        /// <summary>
        /// Checks if two spans are equal in a case-insensitive manner. Utilizes <see cref="StringComparison.OrdinalIgnoreCase" />.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="otherText">The other text.</param>
        /// <returns><c>true</c> if strings are both null or both equal regardless of casing, <c>false</c> otherwise.</returns>
        public static bool EqualsCaseInvariant(this ReadOnlySpan<char> text, ReadOnlySpan<char> otherText)
        {
            if (text.IsEmpty && otherText.IsEmpty)
                return true;

            if (text == otherText)
                return true;

            return MemoryExtensions.Equals(text, otherText, StringComparison.OrdinalIgnoreCase);
        }
    }
}
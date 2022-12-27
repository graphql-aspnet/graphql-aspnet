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
    /// Extension methods for working with <see cref="Span{T}"/>.
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
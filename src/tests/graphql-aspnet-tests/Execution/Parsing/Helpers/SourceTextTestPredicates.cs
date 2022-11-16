// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2.Helpers
{
    using System;
    using GraphQL.AspNet.Parsing2.Lexing.Source;

    /// <summary>
    /// Various test delegates for seting hte NextPhrase method on <see cref="SourceText"/>.
    /// </summary>
    internal class SourceTextTestPredicates
    {
        /// <summary>
        /// Validates that the first and last characters are single quotes.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if the phrase is valid, <c>false</c> otherwise.</returns>
        public static NextPhraseResult SingleQuoteDelimited(ReadOnlySpan<char> text)
        {
            if (text.Length < 2)
                return NextPhraseResult.Continue;

            return text[0] == '\'' && text[text.Length - 1] == '\''
                ? NextPhraseResult.Complete :
                NextPhraseResult.Continue;
        }
    }
}
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Lexing.Helpers
{
    using System;
    using GraphQL.AspNet.Parsing.Lexing.Source;

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
        public static NextPhraseResult SingleQuoteDelimited(in ReadOnlySpan<char> text)
        {
            if (text.Length < 2)
                return NextPhraseResult.Continue;

            return text[0] == '\'' && text[text.Length - 1] == '\''
                ? NextPhraseResult.Complete :
                NextPhraseResult.Continue;
        }
    }
}
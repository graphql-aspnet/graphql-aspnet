// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.Source
{
    using System;

    /// <summary>
    /// A delegate to be used with <see cref="SourceText" /> when performing a text extraction
    /// based on reading a variable number of characters. Return true when the span references a set of characters
    /// to be returned from the source text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>A result of inspecting the phrase instructing the feeder on how to act next.</returns>
    public delegate NextPhraseResult SourceNextPhraseDelegate(in ReadOnlySpan<char> text);
}
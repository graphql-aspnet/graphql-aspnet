// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.Lexing.Source
{
    /// <summary>
    /// A formal delegate for any function wishing to act as a filter on a
    /// token or piece of source text.
    /// </summary>
    /// <param name="text">The character to inspect.</param>
    /// <returns><c>true</c> if the filter was applied, <c>false</c> otherwise.</returns>
    internal delegate bool SourceTextNextCharacterFilterDelegate(char text);
}
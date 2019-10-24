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
    /// <summary>
    /// A reference to where in the source text the seek operation should begin from.
    /// </summary>
    public enum SourceTextPosition
    {
        FromCurrentCursor,
        FromStart,
    }
}
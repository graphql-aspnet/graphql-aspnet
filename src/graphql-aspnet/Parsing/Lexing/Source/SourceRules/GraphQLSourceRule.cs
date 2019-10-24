// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Parsing.Lexing.Source.SourceRules
{
    /// <summary>
    /// A list of rules that a <see cref="SourceText"/> can process, at its current location.
    /// </summary>
    public enum GraphQLSourceRule
    {
        IsStartOfNameGlyph,
        IsControlGlyph,
        IsCommentGlyph,
        IsStringDelimiterGlyph,
        IsStartOfNumberGlyph,
    }
}
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
    /// Implemented by classes wishing to act as a source rule for validating <see cref="SourceText"/>.
    /// </summary>
    public interface ISourceRule
    {
        /// <summary>
        /// Validates the text, from its current position, passes the rule.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        bool Validate(SourceText text);
    }
}
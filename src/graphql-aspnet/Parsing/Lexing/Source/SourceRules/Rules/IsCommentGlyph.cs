// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.Source.SourceRules.Rules
{
    using GraphQL.AspNet.Parsing.Lexing.Tokens;

    /// <summary>
    /// Checks to see if the source text is currently pointed at the start of a comment or not.
    /// </summary>
    /// <seealso cref="ISourceRule" />
    public class IsCommentGlyph : ISourceRule
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISourceRule Instance { get; } = new IsCommentGlyph();

        /// <summary>
        /// Prevents a default instance of the <see cref="IsCommentGlyph"/> class from being created.
        /// </summary>
        private IsCommentGlyph()
        {
        }

        /// <summary>
        /// Validates the text, from its current position, passes the rule.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public bool Validate(SourceText text)
        {
            return text.Peek() == (int)TokenType.Comment;
        }
    }
}
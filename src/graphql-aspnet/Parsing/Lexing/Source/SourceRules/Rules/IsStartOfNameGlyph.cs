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
    /// <summary>
    /// Validates that a source text is currently pointed at a character that can be the start
    /// of a name block.
    /// </summary>
    /// <seealso cref="ISourceRule" />
    public class IsStartOfNameGlyph : ISourceRule
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static IsStartOfNameGlyph Instance { get; } = new IsStartOfNameGlyph();

        /// <summary>
        /// Prevents a default instance of the <see cref="IsStartOfNameGlyph"/> class from being created.
        /// </summary>
        private IsStartOfNameGlyph()
        {
        }

        /// <summary>
        /// Validates the text, from its current position, passes the rule.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <returns>
        ///   <c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public bool Validate(SourceText text)
        {
            var c = text.Peek();

            return c == '_' || char.IsLetter(c);
        }
    }
}
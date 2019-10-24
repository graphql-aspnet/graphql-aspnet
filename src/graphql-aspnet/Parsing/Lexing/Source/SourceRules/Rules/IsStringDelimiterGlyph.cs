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
    /// A rule that determines if the character on the glyph is a string delimiter or not (the double quote).
    /// </summary>
    public class IsStringDelimiterGlyph : ISourceRule
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static IsStringDelimiterGlyph Instance { get; } = new IsStringDelimiterGlyph();

        /// <summary>
        /// Prevents a default instance of the <see cref="IsStringDelimiterGlyph"/> class from being created.
        /// </summary>
        private IsStringDelimiterGlyph()
        {
        }

        /// <summary>
        /// Validates the text, from its current position, passes the rule.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public bool Validate(SourceText text)
        {
            if (!text.HasData || text.Peek() != ParserConstants.Characters.DOUBLE_QUOTE)
                return false;

            // ensure its not an escaped double quote
            return text.Cursor == 0 || text.Peek(-1)[0] != '\\';
        }
    }
}
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
    /// A rule that determines if the character pointed to by the source can be the start of a number.
    /// </summary>
    /// <seealso cref="ISourceRule" />
    public class IsStartOfNumberGlyph : ISourceRule
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISourceRule Instance { get; } = new IsStartOfNumberGlyph();

        /// <summary>
        /// Prevents a default instance of the <see cref="IsStartOfNumberGlyph"/> class from being created.
        /// </summary>
        private IsStartOfNumberGlyph()
        {
        }

        /// <summary>
        /// Validates the text, from its current position, passes the rule.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public bool Validate(SourceText text)
        {
            if (!text.HasData)
                return false;

            var c = text.Peek();

            // numbers must start with a negative sign or a number
            return c == '-' || char.IsDigit(c);
        }
    }
}
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
    using GraphQL.AspNet.Parsing.Lexing.CharacterGroupValidation;

    /// <summary>
    /// A rule validating if the current character is an operational character or just part of a name. Makes
    /// no assumption as to nesting (for example inside a string).
    /// </summary>
    /// <seealso cref="ISourceRule" />
    internal class IsControlGlyph : ISourceRule
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISourceRule Instance { get; } = new IsControlGlyph();

        /// <summary>
        /// Prevents a default instance of the <see cref="IsControlGlyph"/> class from being created.
        /// </summary>
        private IsControlGlyph()
        {
        }

        /// <summary>
        /// Validates the text, from its current position, passes the rule.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public bool Validate(SourceText text)
        {
            return ControlPhraseValidator.IsAcceptedTokenGlyph(text.Peek());
        }
    }
}
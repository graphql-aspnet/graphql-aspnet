// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// A set of extensions containing logic pertaining to the <see cref="TokenType"/> used by the lexer.
    /// </summary>
    public static class TokenTypeExtensions
    {
        private static readonly Dictionary<TokenType, ReadOnlyMemory<char>> _descriptions = new Dictionary<TokenType, ReadOnlyMemory<char>>();

        /// <summary>
        /// Initializes static members of the <see cref="TokenTypeExtensions"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">Enum item {tokenType.Name}.{tokenValue.ToString()}.</exception>
        static TokenTypeExtensions()
        {
            var tokenType = typeof(TokenType);
            foreach (var tokenValue in (TokenType[])Enum.GetValues(typeof(TokenType)))
            {
                var fi = tokenType.GetField(tokenValue.ToString());
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                    _descriptions.Add(tokenValue, attributes[0].Description.AsMemory());
                else
                    throw new InvalidOperationException($"Enum item {tokenType.Name}.{tokenValue.ToString()} does not declare a description.");
            }
        }

        /// <summary>
        /// Determines whether the provided <see cref="char" /> represents a character marked as a controller character or just some
        /// other named character.
        /// </summary>
        /// <param name="charCode">The character code.</param>
        /// <returns><c>true</c> if the token type represents a control glyph otherwise, <c>false</c>.</returns>
        public static bool IsControlerGlyph(char charCode)
        {
            switch ((int)charCode)
            {
                case (int)TokenType.Comment:
                case (int)TokenType.Bang:
                case (int)TokenType.Dollar:
                case (int)TokenType.ParenLeft:
                case (int)TokenType.ParenRight:
                case (int)TokenType.EqualsSign:
                case (int)TokenType.SpreadOperatorInitiator:
                case (int)TokenType.Colon:
                case (int)TokenType.AtSymbol:
                case (int)TokenType.BracketLeft:
                case (int)TokenType.BracketRight:
                case (int)TokenType.CurlyBraceLeft:
                case (int)TokenType.CurlyBraceRight:
                case (int)TokenType.Pipe:
                case (int)TokenType.Comma:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Attempts to convert the given character slice to its equivilant control character enumeration....if any.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>TokenType.</returns>
        public static TokenType ToTokenType(this in ReadOnlySpan<char> text)
        {
            if (text.Length == 3 && text.Equals(ParserConstants.Characters.SpreadOperator.Span, StringComparison.OrdinalIgnoreCase))
                return TokenType.SpreadOperator;

            if (text.Length != 1)
                return TokenType.None;

            if (Enum.IsDefined(typeof(TokenType), (int)text[0]))
                return (TokenType)text[0];

            return TokenType.None;
        }

        /// <summary>
        /// Attempts to convert the given character slice to its equivilant control character enumeration....if any.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>TokenType.</returns>
        public static TokenType ToTokenType(this in ReadOnlyMemory<char> text)
        {
            return text.Span.ToTokenType();
        }

        /// <summary>
        /// Returns the description assigned to this value.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <returns>System.String.</returns>
        public static ReadOnlyMemory<char> Description(this TokenType tokenType)
        {
            return _descriptions[tokenType];
        }
    }
}
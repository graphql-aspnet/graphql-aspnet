﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.Lexing.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// A set of extensions containing logic pertaining to the <see cref="TokenType"/> used by the lexer.
    /// </summary>
    internal static class TokenTypeExtensions
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
                    _descriptions.Add(tokenValue, string.Empty.AsMemory());
            }
        }

        /// <summary>
        /// Determines whether the provided <see cref="char" /> represents a character marked as a controller character or just some
        /// other named character.
        /// </summary>
        /// <param name="charCode">The character code.</param>
        /// <returns><c>true</c> if the token type represents a control glyph otherwise, <c>false</c>.</returns>
        public static bool IsControllerGlyph(char charCode)
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
        /// Determines whether the supplied <see cref="char"/> represents a valid
        /// token type.
        /// </summary>
        /// <remarks>
        /// Given the finite list of single character token types
        /// this method allows for an easy to use, zero heap allocation method
        /// for deteremining a <see cref="char"/> to <see cref="TokenType"/> mapping.</remarks>
        /// <param name="charCode">The character code.</param>
        /// <returns><c>true</c> if [is token type] [the specified character code]; otherwise, <c>false</c>.</returns>
        public static TokenType ToTokenType(this char charCode)
        {
            switch ((TokenType)charCode)
            {
                case TokenType.None:
                case TokenType.Comment:
                case TokenType.Bang:
                case TokenType.Dollar:
                case TokenType.ParenLeft:
                case TokenType.ParenRight:
                case TokenType.Comma:
                case TokenType.EqualsSign:
                case TokenType.SpreadOperatorInitiator:
                case TokenType.Colon:
                case TokenType.AtSymbol:
                case TokenType.BracketLeft:
                case TokenType.BracketRight:
                case TokenType.CurlyBraceLeft:
                case TokenType.CurlyBraceRight:
                case TokenType.Pipe:
                    return (TokenType)charCode;

                default:
                    return TokenType.None;
            }
        }

        /// <summary>
        /// Attempts to convert the given character slice to its equivilant control character enumeration....if any.
        /// </summary>
        /// <param name="text">The text to inspect.</param>
        /// <returns>TokenType.</returns>
        public static TokenType ToTokenType(this in ReadOnlySpan<char> text)
        {
            if (text.Length == 3
                && text.Equals(ParserConstants.Characters.SpreadOperator.Span, StringComparison.OrdinalIgnoreCase))
                return TokenType.SpreadOperator;

            if (text.Length != 1)
                return TokenType.None;

            return text[0].ToTokenType();
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
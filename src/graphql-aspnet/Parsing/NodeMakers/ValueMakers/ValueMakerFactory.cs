// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.NodeMakers.ValueMakers
{
    using System;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;

    /// <summary>
    /// A factory for managing which <see cref="ISyntaxNodeMaker"/> is appropriate for
    /// the processing the requested value node generation.
    /// </summary>
    public static class ValueMakerFactory
    {
        /// <summary>
        /// Creates a <see cref="ISyntaxNodeMaker"/> that interpretes the given token
        /// as though it were a value to be created and returns the correct maker
        /// to fulfill the request.
        /// </summary>
        /// <param name="token">The token representing the start of the value to be parsed.</param>
        /// <returns>A <see cref="ISyntaxNodeMaker"/> that can parse the data.</returns>
        public static ISyntaxNodeMaker CreateMaker(LexicalToken token)
        {
            // an input value could be:
            // NameToken:    a potential enumeration or true|false
            // StringToken:  some string value
            // NumberToken:  some number value
            // CurlyLeft:    complex JSON object
            // BracketLeft:  an array of items
            // DollarSign:   variable reference
            if (token is NameToken)
            {
                if (token.Text.Span.Equals(ParserConstants.Keywords.True.Span, StringComparison.Ordinal) ||
                    token.Text.Span.Equals(ParserConstants.Keywords.False.Span, StringComparison.Ordinal))
                {
                    return BooleanValueNodeMaker.Instance;
                }
                else
                {
                    return EnumValueNodeMaker.Instance;
                }
            }

            if (token is NullToken)
                return NullValueNodeMaker.Instance;

            if (token is StringToken)
                return StringValueNodeMaker.Instance;

            if (token is NumberToken)
                return NumberValueNodeMaker.Instance;

            if (token.TokenType == TokenType.CurlyBraceLeft)
                return ComplexValueNodeMaker.Instance;

            if (token.TokenType == TokenType.BracketLeft)
                return ListValueNodeMaker.Instance;

            if (token.TokenType == TokenType.Dollar)
                return VariableValueNodeMaker.Instance;

            return null;
        }
    }
}
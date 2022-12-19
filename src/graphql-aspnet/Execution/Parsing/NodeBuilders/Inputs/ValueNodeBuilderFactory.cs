// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.NodeBuilders.Inputs
{
    using System;
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A factory that will choose the correct builder to build the proper value
    /// node from hte token stream.
    /// </summary>
    internal class ValueNodeBuilderFactory
    {
        /// <summary>
        /// Inspects the token stream and generates the appropriate builder to
        /// build the next syntax node.
        /// </summary>
        /// <param name="tokenStream">The token stream to inspect.</param>
        /// <returns>ISyntaxNodeBuilder.</returns>
        public static ISyntaxNodeBuilder CreateBuilder(TokenStream tokenStream)
        {
            // an input value could be:
            // NameToken:    a potential enumeration or true|false
            // StringToken:  some string value
            // NumberToken:  some number value
            // CurlyLeft:    complex JSON object
            // BracketLeft:  an array of items
            // DollarSign:   variable reference
            var token = tokenStream.ActiveToken;
            if (token.TokenType == TokenType.Name)
            {
                var text = tokenStream.Source.Slice(token.Block);
                if (text.Equals(ParserConstants.Keywords.True.Span, StringComparison.Ordinal) ||
                    text.Equals(ParserConstants.Keywords.False.Span, StringComparison.Ordinal))
                {
                    return BooleanValueNodeBuilder.Instance;
                }
                else
                {
                    return EnumValueNodeBuilder.Instance;
                }
            }

            if (token.TokenType == TokenType.Null)
                return NullValueNodeBuilder.Instance;

            if (token.TokenType == TokenType.String)
                return StringValueNodeBuilder.Instance;

            if (token.TokenType == TokenType.Float || token.TokenType == TokenType.Integer)
                return NumberValueNodeBuilder.Instance;

            if (token.TokenType == TokenType.CurlyBraceLeft)
                return ComplexValueNodeBuilder.Instance;

            if (token.TokenType == TokenType.BracketLeft)
                return ListValueNodeBuilder.Instance;

            if (token.TokenType == TokenType.Dollar)
                return VariableValueNodeBuilder.Instance;

            return null;
        }
    }
}
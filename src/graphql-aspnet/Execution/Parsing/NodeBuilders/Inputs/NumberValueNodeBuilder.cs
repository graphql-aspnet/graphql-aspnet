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
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A syntax node builder that creates number scalar value nodes from a token stream.
    /// </summary>
    internal class NumberValueNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new NumberValueNodeBuilder();

        private NumberValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            SyntaxNode numberNode = default;
            if (tokenStream.Match(TokenType.Null))
            {
                numberNode = new SyntaxNode(
                    SyntaxNodeType.ScalarValue,
                    tokenStream.Location,
                    new SyntaxNodeValue(
                        tokenStream.ActiveToken.Block,
                        ScalarValueType.Number));
            }
            else
            {
                tokenStream.MatchOrThrow(TokenType.Float, TokenType.Integer);
                numberNode = new SyntaxNode(
                    SyntaxNodeType.ScalarValue,
                    tokenStream.Location,
                    new SyntaxNodeValue(
                        tokenStream.ActiveToken.Block,
                        ScalarValueType.Number));
            }

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref numberNode);
            tokenStream.Next();
        }
    }
}
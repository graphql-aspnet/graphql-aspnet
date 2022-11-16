// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.NodeBuilders.Inputs
{
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    internal class StringValueNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new StringValueNodeBuilder();

        private StringValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            SyntaxNode stringNode = default;
            if (tokenStream.Match(TokenType.Null))
            {
                stringNode = new SyntaxNode(
                    SyntaxNodeType.ScalarValue,
                    tokenStream.Location,
                    new SyntaxNodeValue(
                        tokenStream.ActiveToken.Block,
                        ScalarValueType.String));
            }
            else
            {
                tokenStream.MatchOrThrow(TokenType.String);
                stringNode = new SyntaxNode(
                   SyntaxNodeType.ScalarValue,
                   tokenStream.Location,
                   new SyntaxNodeValue(
                       tokenStream.ActiveToken.Block,
                       ScalarValueType.String));
            }

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref stringNode);
            tokenStream.Next();
        }
    }
}
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

    internal class NumberValueNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new NumberValueNodeBuilder();

        private NumberValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            SynNode numberNode = default;
            if (tokenStream.Match(TokenType.Null))
            {
                numberNode = new SynNode(
                    SynNodeType.ScalarValue,
                    tokenStream.Location,
                    new SynNodeValue(
                        tokenStream.ActiveToken.Text,
                        ScalarValueType.Number));
            }
            else
            {
                tokenStream.MatchOrThrow(TokenType.Float, TokenType.Integer);
                numberNode = new SynNode(
                    SynNodeType.ScalarValue,
                    tokenStream.Location,
                    new SynNodeValue(
                        tokenStream.ActiveToken.Text,
                        ScalarValueType.Number));
            }

            synTree = synTree.AddChildNode(ref parentNode, ref numberNode);
            tokenStream.Next();
        }
    }
}
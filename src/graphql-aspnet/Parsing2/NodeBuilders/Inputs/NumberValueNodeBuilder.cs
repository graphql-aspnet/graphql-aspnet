namespace GraphQL.AspNet.Parsing2.NodeBuilders.Inputs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

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

            synTree = synTree.AddChildNode(ref parentNode,ref numberNode);
            tokenStream.Next();
        }
    }
}

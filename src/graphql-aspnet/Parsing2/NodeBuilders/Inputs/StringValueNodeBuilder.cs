﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.NodeBuilders.Inputs
{
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    internal class StringValueNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new StringValueNodeBuilder();

        private StringValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            SynNode stringNode = default;
            if (tokenStream.Match(TokenType.Null))
            {
                stringNode = new SynNode(
                    SynNodeType.ScalarValue,
                    tokenStream.Location,
                    new SynNodeValue(
                        tokenStream.ActiveToken.Text,
                        ScalarValueType.String));
            }
            else
            {
                tokenStream.MatchOrThrow(TokenType.String);
                stringNode = new SynNode(
                   SynNodeType.ScalarValue,
                   tokenStream.Location,
                   new SynNodeValue(
                       tokenStream.ActiveToken.Text,
                       ScalarValueType.String));
            }

            synTree = synTree.AddChildNode(ref parentNode, ref stringNode);
            tokenStream.Next();
        }
    }
}
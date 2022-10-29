﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.NodeMakers
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A maker for generating a single, fully qualified, <see cref="InputItemNode"/>,
    /// a combination name and value.
    /// </summary>
    public class InputItemNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new InputItemNodeMaker();

        /// <summary>
        /// Prevents a default instance of the <see cref="InputItemNodeMaker"/> class from being created.
        /// </summary>
        private InputItemNodeMaker()
        {
        }

        /// <summary>
        /// Processes the queue as far as it needs to to generate a fully qualiffied
        /// <see cref="SyntaxNode" /> based on its ruleset.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>LexicalToken.</returns>
        public SyntaxNode MakeNode(ISyntaxNodeList nodeList, ref TokenStream tokenStream)
        {
            // ensure we're pointing at a potential input item
            tokenStream.MatchOrThrow(TokenType.Name);
            var startLocation = tokenStream.Location;

            var name = tokenStream.ActiveToken.Text;
            tokenStream.Next();

            // input values are in the format:   NameToken: ValueToken
            // ensure and consume the colon
            tokenStream.MatchOrThrow(TokenType.Colon);
            tokenStream.Next();

            var collectionId = nodeList.BeginTempCollection();

            var maker = NodeMakerFactory.CreateMaker<InputValueNode>();
            var value = maker.MakeNode(nodeList, ref tokenStream);

            nodeList.AddNodeToTempCollection(collectionId, value);

            // account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirMaker = NodeMakerFactory.CreateMaker<DirectiveNode>();

                do
                {
                    var directive = dirMaker.MakeNode(nodeList, ref tokenStream);
                    nodeList.AddNodeToTempCollection(collectionId, directive);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            var node = new InputItemNode(startLocation, name);
            node.SetChildren(nodeList, collectionId);

            return node;
        }
    }
}
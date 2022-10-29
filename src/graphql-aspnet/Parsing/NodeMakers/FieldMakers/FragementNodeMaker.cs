﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.NodeMakers.FieldMakers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using KEYWORDS = GraphQL.AspNet.Parsing.ParserConstants.Keywords;

    /// <summary>
    /// A node maker that, in context, can decern a pointer to a named fragment from an
    /// inline type-specifc fragment and generate the correct node from the stream data
    /// as required.
    /// </summary>
    public class FragementNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new FragementNodeMaker();

        /// <summary>
        /// Prevents a default instance of the <see cref="FragementNodeMaker"/> class from being created.
        /// </summary>
        private FragementNodeMaker()
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
            tokenStream.MatchOrThrow(TokenType.SpreadOperator);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            SyntaxNode node;
            SyntaxNode collection = null;
            ReadOnlyMemory<char> fragmentName = ReadOnlyMemory<char>.Empty;
            ReadOnlyMemory<char> restrictedToType = ReadOnlyMemory<char>.Empty;

            var collectionId = nodeList.BeginTempCollection();
            int childNodesCreated = 0;

            // check for inline fragment first "on Type"
            if (tokenStream.Match(KEYWORDS.On))
            {
                tokenStream.Next();
                tokenStream.MatchOrThrow(TokenType.Name);
                restrictedToType = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            // might be a named fragment?
            if (tokenStream.Match(TokenType.Name))
            {
                fragmentName = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            // account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirMaker = NodeMakerFactory.CreateMaker<DirectiveNode>();

                do
                {
                    var directive = dirMaker.MakeNode(nodeList, ref tokenStream);
                    nodeList.AddNodeToTempCollection(collectionId, directive);
                    childNodesCreated++;
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            // may contain a field set
            if (tokenStream.Match(TokenType.CurlyBraceLeft))
            {
                var filedColMaker = NodeMakerFactory.CreateMaker<FieldCollectionNode>();
                collection = filedColMaker.MakeNode(nodeList, ref tokenStream);
                if (collection.Children != null)
                {
                    nodeList.AddNodeToTempCollection(collectionId, collection);
                    childNodesCreated++;
                }
            }

            if (fragmentName.IsEmpty && restrictedToType.IsEmpty && childNodesCreated == 0)
            {
                throw new GraphQLSyntaxException(
                    startLocation,
                    "Invalid fragment syntax. No fragment could be created from the supplied block.");
            }

            if (!fragmentName.IsEmpty)
                node = new FragmentSpreadNode(startLocation, fragmentName);
            else
                node = new InlineFragmentNode(startLocation, restrictedToType);

            node.SetChildren(nodeList, collectionId);

            return node;
        }
    }
}
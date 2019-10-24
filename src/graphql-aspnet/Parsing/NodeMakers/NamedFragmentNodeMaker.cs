// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.NodeMakers
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using KEYWORDS = GraphQL.AspNet.Parsing.ParserConstants.Keywords;

    /// <summary>
    /// A maker that can generate a graphql Fragment
    /// spec: https://graphql.github.io/graphql-spec/June2018/#sec-Language.Fragments .
    /// </summary>
    public class NamedFragmentNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new NamedFragmentNodeMaker();

        /// <summary>
        /// Prevents a default instance of the <see cref="NamedFragmentNodeMaker"/> class from being created.
        /// </summary>
        private NamedFragmentNodeMaker()
        {
        }

        /// <summary>
        /// Makes the node.
        /// </summary>
        /// <param name="tokenStream">The token queue.</param>
        /// <returns>SyntaxNode.</returns>
        public SyntaxNode MakeNode(TokenStream tokenStream)
        {
            // a root fragment must be in the form of keywords:  fragment on TargetType{}

            // "fragment" keyword
            var startLocation = tokenStream.Location;
            tokenStream.MatchOrThrow(KEYWORDS.Fragment);
            tokenStream.Next();

            // name of the fragment
            tokenStream.MatchOrThrow<NameToken>();
            var fragmentName = tokenStream.ActiveToken.Text;
            tokenStream.Next();

            // "on" keyword
            var targetType = ReadOnlyMemory<char>.Empty;
            if (tokenStream.Match(KEYWORDS.On))
            {
                tokenStream.Next();

                // target type
                tokenStream.MatchOrThrow<NameToken>();
                targetType = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            // account for possible directives on this field
            var directives = new List<SyntaxNode>();
            while (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirMaker = NodeMakerFactory.CreateMaker<DirectiveNode>();
                var directive = dirMaker.MakeNode(tokenStream);
                directives.Add(directive);
            }

            // must be pointing at the fragment field set now
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);
            var fieldCollectionMaker = NodeMakerFactory.CreateMaker<FieldCollectionNode>();
            var collection = fieldCollectionMaker.MakeNode(tokenStream);

            var node = new NamedFragmentNode(startLocation, fragmentName, targetType);
            if (collection.Children.Count > 0)
                node.AddChild(collection);

            foreach (var directive in directives)
                node.AddChild(directive);

            return node;
        }
    }
}
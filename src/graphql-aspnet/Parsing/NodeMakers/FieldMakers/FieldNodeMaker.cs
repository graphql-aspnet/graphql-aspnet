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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// a node maker, expecting to start at a name token on a stream
    /// and will parse the stream in an attempt to extract a single, qualified field reference.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class FieldNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new FieldNodeMaker();

        private FieldNodeMaker()
        {
        }

        /// <summary>
        /// Processes the queue as far as it needs to to generate a fully qualiffied
        /// <see cref="SyntaxNode" /> based on its ruleset.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>LexicalToken.</returns>
        public SyntaxNode MakeNode(TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.Name);

            var startLocation = tokenStream.Location;
            var fieldName = tokenStream.ActiveToken.Text;
            var fieldAlias = fieldName;
            tokenStream.Next();
            SyntaxNode inputCollection = null;
            SyntaxNode fieldCollection = null;
            List<SyntaxNode> directives = null;

            // account for a possible alias on the field name
            if (tokenStream.Match(TokenType.Colon))
            {
                tokenStream.Next();
                tokenStream.MatchOrThrow(TokenType.Name);

                fieldName = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            // account for possible collection of input values
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var maker = NodeMakerFactory.CreateMaker<InputItemCollectionNode>();
                inputCollection = maker.MakeNode(tokenStream);
            }

            // account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var maker = NodeMakerFactory.CreateMaker<DirectiveNode>();
                directives = new List<SyntaxNode>();

                do
                {
                    var directive = maker.MakeNode(tokenStream);
                    directives.Add(directive);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            // account for posible field collection on this field
            if (tokenStream.Match(TokenType.CurlyBraceLeft))
            {
                var maker = NodeMakerFactory.CreateMaker<FieldCollectionNode>();
                fieldCollection = maker.MakeNode(tokenStream);
            }

            var node = new FieldNode(startLocation, fieldAlias, fieldName);

            if (inputCollection != null)
                node.AddChild(inputCollection);

            if (directives != null)
            {
                foreach (var directive in directives)
                    node.AddChild(directive);
            }

            if (fieldCollection?.Children != null)
                node.AddChild(fieldCollection);

            return node;
        }
    }
}
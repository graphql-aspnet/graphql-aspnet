// *************************************************************
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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A maker responsible for processing an entire collection of field nodes from an opening '{'
    /// to its matching closing '}'.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class FieldCollectionNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new FieldCollectionNodeMaker();

        private FieldCollectionNodeMaker()
        {
        }

        /// <summary>
        /// Processes the queue as far as it needs to to generate a fully qualiffied
        /// <see cref="SyntaxNode" /> based on its ruleset.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>LexicalToken.</returns>
        public SyntaxNode MakeNode(ref TokenStream tokenStream)
        {
            // the token stream MUST be positioned at an open curley brace for this to function
            // correclty
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            List<SyntaxNode> childNodes = null;
            if (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.CurlyBraceRight))
            {
                childNodes = new List<SyntaxNode>();
                do
                {
                    var maker = this.CreateFieldMaker(ref tokenStream);
                    var node = maker.MakeNode(ref tokenStream);
                    childNodes.Add(node);
                }
                while (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.CurlyBraceRight));
            }

            // ensure and move past the close curly brace of the collection
            tokenStream.MatchOrThrow(TokenType.CurlyBraceRight);
            tokenStream.Next();

            var collectionNode = new FieldCollectionNode(startLocation);

            if (childNodes != null)
            {
                foreach (var field in childNodes)
                    collectionNode.AddChild(field);
            }

            return collectionNode;
        }

        /// <summary>
        /// Factory method for chosing hte correct field maker given the current
        /// state of the stream.
        /// </summary>
        /// <param name="tokenStream">The token stream to inspect.</param>
        /// <returns>An <see cref="ISyntaxNodeMaker"/> that can create a field node
        /// from the stream contents.</returns>
        private ISyntaxNodeMaker CreateFieldMaker(ref TokenStream tokenStream)
        {
            if (tokenStream.Match(TokenType.SpreadOperator))
                return FragementNodeMaker.Instance;
            else
                return FieldNodeMaker.Instance;
        }
    }
}
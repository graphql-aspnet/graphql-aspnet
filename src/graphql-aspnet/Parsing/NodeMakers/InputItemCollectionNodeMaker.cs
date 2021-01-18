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
    using System.Collections.Generic;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A maker that can process a collection of values, supplied on a <see cref="FieldNode"/>
    /// and parse them into valid syntax tree elements.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class InputItemCollectionNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new InputItemCollectionNodeMaker();

        private InputItemCollectionNodeMaker()
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
            // parentleft:  input item collection on a field
            // curlybraceleft: input item collection in a complex field
            tokenStream.MatchOrThrow(TokenType.ParenLeft, TokenType.CurlyBraceLeft);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            var maker = NodeMakerFactory.CreateMaker<InputItemNode>();
            var children = new List<SyntaxNode>();
            while (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.ParenRight, TokenType.CurlyBraceRight))
            {
                // ensure each input value in this collection is unique
                if (maker.MakeNode(tokenStream) is InputItemNode node)
                {
                    children.Add(node);
                }
            }

            // ensure the paren right is being pointed at in the stream
            tokenStream.MatchOrThrow(TokenType.ParenRight, TokenType.CurlyBraceRight);
            tokenStream.Next();

            var collection = new InputItemCollectionNode(startLocation);
            foreach (var child in children)
                collection.AddChild(child);

            return collection;
        }
    }
}
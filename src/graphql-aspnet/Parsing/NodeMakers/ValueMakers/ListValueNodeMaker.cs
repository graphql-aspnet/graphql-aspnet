// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.NodeMakers.ValueMakers
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An intermediate value maker that generates a value reference to an array/list of other input values.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class ListValueNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the singleton instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new ListValueNodeMaker();

        private ListValueNodeMaker()
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
            var children = new List<SyntaxNode>();
            var startLocation = tokenStream.Location;
            tokenStream.MatchOrThrow(TokenType.BracketLeft);
            tokenStream.Next();

            while (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.BracketRight))
            {
                var childMaker = ValueMakerFactory.CreateMaker(tokenStream.ActiveToken);
                if (childMaker == null)
                {
                    throw new GraphQLSyntaxException(
                        tokenStream.Location,
                        $"Unexpected token in list, no value could be parsed. Expected '{TokenType.BracketRight.Description().ToString()}' or an " +
                        $"input object value but receieved '{tokenStream.ActiveToken.Text.ToString()}'");
                }

                var childNode = childMaker.MakeNode(tokenStream);
                children.Add(childNode);
            }

            // ensure we are pointing at an end of list
            tokenStream.MatchOrThrow(TokenType.BracketRight);
            tokenStream.Next();

            var listNode = new ListValueNode(startLocation);
            listNode.Children.AddRange(children);

            return listNode;
        }
    }
}
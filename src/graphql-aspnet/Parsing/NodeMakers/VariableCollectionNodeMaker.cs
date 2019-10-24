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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;

    /// <summary>
    /// A maker responsible for parsing the stream through a collection of top level
    /// variables. Expects the stream to point to an open paren and will process just past
    /// its matching close paren.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class VariableCollectionNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new VariableCollectionNodeMaker();

        private VariableCollectionNodeMaker()
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
            // the token stream MUST be positioned at an open paren for this maker to function correclty
            tokenStream.MatchOrThrow(TokenType.ParenLeft);

            var collection = new VariableCollectionNode(tokenStream.Location);
            tokenStream.Next();

            var variableMaker = NodeMakerFactory.CreateMaker<VariableNode>();
            while (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.ParenRight))
            {
                var variable = variableMaker.MakeNode(tokenStream);
                collection.AddChild(variable);
            }

            // ensure and move past the close paren
            tokenStream.MatchOrThrow(TokenType.ParenRight);
            tokenStream.Next();
            return collection;
        }
    }
}
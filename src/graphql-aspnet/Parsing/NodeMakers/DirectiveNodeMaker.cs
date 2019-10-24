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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A maker capable of analzing a token stream to generate a syntax node
    /// to ecapsulate a directive.
    /// </summary>
    public class DirectiveNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new DirectiveNodeMaker();

        /// <summary>
        /// Prevents a default instance of the <see cref="DirectiveNodeMaker"/> class from being created.
        /// </summary>
        private DirectiveNodeMaker()
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
            tokenStream.MatchOrThrow(TokenType.AtSymbol);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            tokenStream.MatchOrThrow<NameToken>();
            var directiveName = tokenStream.ActiveToken.Text;
            tokenStream.Next();

            // after the directive name an input collection may exist, parse it out
            SyntaxNode inputCol = null;
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var inputMaker = NodeMakerFactory.CreateMaker<InputItemCollectionNode>();
                inputCol = inputMaker.MakeNode(tokenStream);
            }

            // assemble the directive
            var directive = new DirectiveNode(startLocation, directiveName);
            if (inputCol != null)
                directive.AddChild(inputCol);

            return directive;
        }
    }
}
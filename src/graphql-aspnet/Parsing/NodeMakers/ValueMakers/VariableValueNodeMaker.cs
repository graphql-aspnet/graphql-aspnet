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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A maker for generating an input value that is a <see cref="VariableValueNode"/>.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class VariableValueNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <value>the singlton instance of the maker.</value>
        public static ISyntaxNodeMaker Instance { get; } = new VariableValueNodeMaker();

        /// <summary>
        /// Prevents a default instance of the <see cref="VariableValueNodeMaker"/> class from being created.
        /// </summary>
        private VariableValueNodeMaker()
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
            tokenStream.MatchOrThrow(TokenType.Dollar);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            tokenStream.MatchOrThrow<NameToken>();
            var name = tokenStream.ActiveToken.Text;
            tokenStream.Next();

            return new VariableValueNode(startLocation, name);
        }
    }
}
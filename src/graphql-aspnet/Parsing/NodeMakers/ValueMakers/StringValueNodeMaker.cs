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
    /// A node maker that interpretes the stream's active token a provided flat string
    /// value and generates a <see cref="ScalarValueNode"/> to ecapsulate it.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class StringValueNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the singleton instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new StringValueNodeMaker();

        private StringValueNodeMaker()
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
            SyntaxNode node;
            if (tokenStream.Match<NullToken>())
            {
                node = new ScalarValueNode(
                    tokenStream.Location,
                    ScalarValueType.String,
                    tokenStream.ActiveToken.Text);
            }
            else
            {
                tokenStream.MatchOrThrow<StringToken>();
                node = new ScalarValueNode(tokenStream.Location, ScalarValueType.String, tokenStream.ActiveToken.Text);
            }

            tokenStream.Next();
            return node;
        }
    }
}
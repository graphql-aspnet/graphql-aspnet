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
    /// A maker that can generated any form of named or predetermined value.
    /// i.e. booleans, enumerations.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class EnumValueNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the singleton instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new EnumValueNodeMaker();

        /// <summary>
        /// Prevents a default instance of the <see cref="EnumValueNodeMaker"/> class from being created.
        /// </summary>
        private EnumValueNodeMaker()
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
                node = ValueMakerFactory.CreateMaker(tokenStream.ActiveToken).MakeNode(tokenStream);
            }
            else
            {
                tokenStream.MatchOrThrow<NameToken>();
                node = new EnumValueNode(tokenStream.Location, tokenStream.ActiveToken.Text);
                tokenStream.Next();
            }

            return node;
        }
    }
}
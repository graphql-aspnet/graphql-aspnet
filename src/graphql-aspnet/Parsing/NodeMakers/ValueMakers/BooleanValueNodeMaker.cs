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
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A node maker capable of creating boolean typed scalar values.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class BooleanValueNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new BooleanValueNodeMaker();

        /// <summary>
        /// Processes the stream as far as it needs to to generate a fully qualiffied
        /// <see cref="SyntaxNode" /> based on its internal ruleset. All implementations
        /// are expected to validate the stream is at a location which it can consume
        /// and throw an <see cref="GraphQLSyntaxException" /> as appropriate and consume
        /// the last character in their phrase, leaving the stream primed for the next
        /// maker in the series.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>LexicalToken.</returns>
        public SyntaxNode MakeNode(TokenStream tokenStream)
        {
            SyntaxNode node = null;
            if (tokenStream.Match<NullToken>())
            {
                node = new ScalarValueNode(
                    tokenStream.Location,
                    ScalarValueType.Boolean,
                    ParserConstants.Keywords.Null);
            }
            else
            {
                tokenStream.MatchOrThrow<NameToken>();
                if (tokenStream.Match(ParserConstants.Keywords.True, ParserConstants.Keywords.False))
                {
                    node = new ScalarValueNode(
                        tokenStream.Location,
                        ScalarValueType.Boolean,
                        tokenStream.ActiveToken.Text);
                }
                else
                {
                    GraphQLSyntaxException.ThrowFromExpectation(
                        tokenStream.Location,
                        "{true|false}",
                        tokenStream.ActiveToken.Text.ToString());
                }
            }

            tokenStream.Next();
            return node;
        }
    }
}
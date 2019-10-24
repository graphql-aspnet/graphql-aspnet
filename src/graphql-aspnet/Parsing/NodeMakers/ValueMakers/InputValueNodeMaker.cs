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
    using System;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A top level maker responsible for generating input values of all type complexities.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class InputValueNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new InputValueNodeMaker();

        /// <summary>
        /// Prevents a default instance of the <see cref="InputValueNodeMaker"/> class from being created.
        /// </summary>
        private InputValueNodeMaker()
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
            var maker = ValueMakerFactory.CreateMaker(tokenStream.ActiveToken);
            if (maker != null)
                return maker.MakeNode(tokenStream);

            GraphQLSyntaxException.ThrowFromExpectation(
                tokenStream.Location,
                "<value>",
                tokenStream.TokenType.Description().ToString());

            throw new InvalidOperationException("Critical Failure, this exception should never be reached.");
        }
    }
}
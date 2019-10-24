// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Interfaces
{
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A maker that can generate a valid syntax node by iterating through the provided queue. Each
    /// maker is responsible for validating that it can start processing given the current position
    /// and will consume every token related to its operation, including any "close control" characters
    /// marking the end of a block.
    /// </summary>
    public interface ISyntaxNodeMaker
    {
        /// <summary>
        /// Processes the stream as far as it needs to to generate a fully qualiffied
        /// <see cref="SyntaxNode" /> based on its internal ruleset. All implementations
        /// are expected to validate the stream is at a location which it can consume
        /// and throw an <see cref="GraphQLSyntaxException"/> as appropriate and consume
        /// the last character in their phrase, leaving the stream primed for the next
        /// maker in the series.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>LexicalToken.</returns>
        SyntaxNode MakeNode(TokenStream tokenStream);
    }
}
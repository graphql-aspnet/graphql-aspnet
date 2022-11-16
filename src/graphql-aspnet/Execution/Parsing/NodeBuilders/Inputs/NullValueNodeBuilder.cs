// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.NodeBuilders.Inputs
{
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;

    internal class NullValueNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new NullValueNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="NullValueNodeBuilder" /> class from being created.
        /// </summary>
        private NullValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.Null);
            var nullNode = new SyntaxNode(
                SyntaxNodeType.NullValue,
                tokenStream.Location);

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref nullNode);
            tokenStream.Next();
        }
    }
}
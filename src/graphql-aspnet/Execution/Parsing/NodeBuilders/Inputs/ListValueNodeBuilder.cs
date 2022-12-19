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
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A syntax node builder that creates list value nodes from a token stream.
    /// </summary>
    internal class ListValueNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this builder.
        /// </summary>
        /// <value>The builder instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new ListValueNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="ListValueNodeBuilder"/> class from being created.
        /// </summary>
        private ListValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            var startLocation = tokenStream.Location;

            tokenStream.MatchOrThrow(TokenType.BracketLeft);
            tokenStream.Next();

            var listNode = new SyntaxNode(
                SyntaxNodeType.ListValue,
                startLocation);

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref listNode);

            if (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.BracketRight))
            {
                do
                {
                    var childBuilder = ValueNodeBuilderFactory.CreateBuilder(tokenStream);
                    if (childBuilder == null)
                    {
                        throw new GraphQLSyntaxException(
                            tokenStream.Location,
                            $"Unexpected token in list, no value could be parsed. Expected '{TokenType.BracketRight.Description()}' or an " +
                            $"input object value but receieved '{tokenStream.ActiveToken.Block}'");
                    }

                    childBuilder.BuildNode(ref synTree, ref listNode, ref tokenStream);
                }
                while (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.BracketRight));
            }

            // ensure we are pointing at an end of list
            tokenStream.MatchOrThrow(TokenType.BracketRight);
            tokenStream.Next();
        }
    }
}
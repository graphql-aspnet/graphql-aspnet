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
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A syntax node builder that creates string value nodes from a given token stream.
    /// </summary>
    internal class StringValueNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new StringValueNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="StringValueNodeBuilder"/> class from being created.
        /// </summary>
        private StringValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            SyntaxNode stringNode = default;
            if (tokenStream.Match(TokenType.Null))
            {
                stringNode = new SyntaxNode(
                    SyntaxNodeType.ScalarValue,
                    tokenStream.Location,
                    new SyntaxNodeValue(
                        tokenStream.ActiveToken.Block,
                        ScalarValueType.String));
            }
            else
            {
                tokenStream.MatchOrThrow(TokenType.String);
                stringNode = new SyntaxNode(
                   SyntaxNodeType.ScalarValue,
                   tokenStream.Location,
                   new SyntaxNodeValue(
                       tokenStream.ActiveToken.Block,
                       ScalarValueType.String));
            }

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref stringNode);
            tokenStream.Next();
        }
    }
}
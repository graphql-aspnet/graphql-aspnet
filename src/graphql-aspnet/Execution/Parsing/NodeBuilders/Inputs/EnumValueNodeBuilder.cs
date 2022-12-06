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

    /// <summary>
    /// A syntax node builder that builds enum leaf value nodes from a token stream.
    /// </summary>
    internal class EnumValueNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new EnumValueNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="EnumValueNodeBuilder"/> class from being created.
        /// </summary>
        private EnumValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            if (tokenStream.Match(TokenType.Null))
            {
                ValueNodeBuilderFactory
                    .CreateBuilder(tokenStream)
                    .BuildNode(ref synTree, ref parentNode, ref tokenStream);
            }
            else
            {
                tokenStream.MatchOrThrow(TokenType.Name);

                var enumValueNode = new SyntaxNode(
                    SyntaxNodeType.EnumValue,
                    tokenStream.Location,
                    new SyntaxNodeValue(
                        tokenStream.ActiveToken.Block));

                SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref enumValueNode);
                tokenStream.Next();
            }
        }
    }
}
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.NodeBuilders
{
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A syntax node builder that builds directive nodes from a token stream.
    /// </summary>
    internal class DirectiveNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new DirectiveNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="DirectiveNodeBuilder"/> class from being created.
        /// </summary>
        private DirectiveNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.AtSymbol);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            tokenStream.MatchOrThrow(TokenType.Name);
            var directiveName = tokenStream.ActiveToken.Block;
            tokenStream.Next();

            var directiveNode = new SyntaxNode(
                SyntaxNodeType.Directive,
                startLocation,
                new SyntaxNodeValue(directiveName));

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref directiveNode);

            // after the directive name an input collection may exist, parse it out
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var inputBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.InputItemCollection);
                inputBuilder.BuildNode(ref synTree, ref directiveNode, ref tokenStream);
            }
        }
    }
}
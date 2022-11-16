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

    public class ComplexValueNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new ComplexValueNodeBuilder();

        private ComplexValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);
            var startLocation = tokenStream.Location;

            var itemCollectionBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.InputItemCollection);

            var complexValueNode = new SyntaxNode(
                SyntaxNodeType.ComplexValue,
                startLocation);

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref complexValueNode);

            itemCollectionBuilder.BuildNode(ref synTree, ref complexValueNode, ref tokenStream);
        }
    }
}
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.NodeBuilders.Inputs
{
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;

    public class ComplexValueNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new ComplexValueNodeBuilder();

        private ComplexValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);
            var startLocation = tokenStream.Location;

            var itemCollectionBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.InputItemCollection);

            var complexValueNode = new SynNode(
                SynNodeType.ComplexValue,
                startLocation);

            synTree = synTree.AddChildNode(ref parentNode, ref complexValueNode);

            itemCollectionBuilder.BuildNode(ref synTree, ref complexValueNode, ref tokenStream);
        }
    }
}
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

    internal class NullValueNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new NullValueNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="NullValueNodeBuilder" /> class from being created.
        /// </summary>
        private NullValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.Null);
            var nullNode = new SynNode(
                SynNodeType.NullValue,
                tokenStream.Location);

            synTree = synTree.AddChildNode(ref parentNode, ref nullNode);
            tokenStream.Next();
        }
    }
}
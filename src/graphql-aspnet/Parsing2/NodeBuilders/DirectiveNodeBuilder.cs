// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.NodeBuilders
{
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    public class DirectiveNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new DirectiveNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="DirectiveNodeBuilder"/> class from being created.
        /// </summary>
        private DirectiveNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.AtSymbol);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            tokenStream.MatchOrThrow(TokenType.Name);
            var directiveName = tokenStream.ActiveToken.Block;
            tokenStream.Next();

            var directiveNode = new SynNode(
                SynNodeType.Directive,
                startLocation,
                new SynNodeValue(directiveName));

            SynTreeOperations.AddChildNode(ref synTree, ref parentNode, ref directiveNode);

            // after the directive name an input collection may exist, parse it out
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var inputBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.InputItemCollection);
                inputBuilder.BuildNode(ref synTree, ref directiveNode, ref tokenStream);
            }
        }
    }
}
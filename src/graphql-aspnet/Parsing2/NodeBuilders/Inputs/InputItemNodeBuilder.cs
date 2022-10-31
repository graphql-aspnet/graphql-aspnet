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
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    internal class InputItemNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new InputItemNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="InputItemNodeBuilder"/> class from being created.
        /// </summary>
        private InputItemNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            // ensure we're pointing at a potential input item
            tokenStream.MatchOrThrow(TokenType.Name);
            var startLocation = tokenStream.Location;

            var name = tokenStream.ActiveToken.Text;
            tokenStream.Next();

            // input values are in the format:   NameToken: ValueToken
            // ensure and consume the colon
            tokenStream.MatchOrThrow(TokenType.Colon);
            tokenStream.Next();

            var inputItemNode = new SynNode(
                SynNodeType.InputItem,
                startLocation,
                new SynNodeValue(name));

            synTree = synTree.AddChildNode(ref parentNode, ref inputItemNode);

            var valueBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.InputValue);
            valueBuilder.BuildNode(ref synTree, ref inputItemNode, ref tokenStream);

            //// account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirbuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.Directive);

                do
                {
                    dirbuilder.BuildNode(ref synTree, ref inputItemNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }
        }
    }
}
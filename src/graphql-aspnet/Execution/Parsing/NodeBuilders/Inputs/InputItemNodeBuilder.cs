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
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    internal class InputItemNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new InputItemNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="InputItemNodeBuilder"/> class from being created.
        /// </summary>
        private InputItemNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            // ensure we're pointing at a potential input item
            tokenStream.MatchOrThrow(TokenType.Name);
            var startLocation = tokenStream.Location;

            var name = tokenStream.ActiveToken.Block;
            tokenStream.Next();

            // input values are in the format:   NameToken: ValueToken
            // ensure and consume the colon
            tokenStream.MatchOrThrow(TokenType.Colon);
            tokenStream.Next();

            var inputItemNode = new SyntaxNode(
                SyntaxNodeType.InputItem,
                startLocation,
                new SyntaxNodeValue(name));

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref inputItemNode);

            var valueBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.InputValue);
            valueBuilder.BuildNode(ref synTree, ref inputItemNode, ref tokenStream);

            //// account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirbuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.Directive);

                do
                {
                    dirbuilder.BuildNode(ref synTree, ref inputItemNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }
        }
    }
}
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

    internal class FieldNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new FieldNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="FieldNodeBuilder"/> class from being created.
        /// </summary>
        private FieldNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.Name);

            var startLocation = tokenStream.Location;
            var fieldName = tokenStream.ActiveToken.Text;
            var fieldAlias = fieldName;
            tokenStream.Next();

            // account for a possible alias on the field name
            if (tokenStream.Match(TokenType.Colon))
            {
                tokenStream.Next();
                tokenStream.MatchOrThrow(TokenType.Name);

                fieldName = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            var fieldNode = new SynNode(
                SynNodeType.Field,
                startLocation,
                new SynNodeValue(fieldName),
                new SynNodeValue(fieldAlias));

            synTree = synTree.AddChildNode(ref parentNode, ref fieldNode);

            // account for possible collection of input values
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var builder = NodeBuilderFactory.CreateBuilder(SynNodeType.InputItemCollection);
                builder.BuildNode(ref synTree, ref fieldNode, ref tokenStream);
            }

            // account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.Directive);

                do
                {
                    dirBuilder.BuildNode(ref synTree, ref fieldNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            // account for posible field collection on this field
            if (tokenStream.Match(TokenType.CurlyBraceLeft))
            {
                var fieldCollectionBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.FieldCollection);
                fieldCollectionBuilder.BuildNode(ref synTree, ref fieldNode, ref tokenStream);
            }
        }
    }
}
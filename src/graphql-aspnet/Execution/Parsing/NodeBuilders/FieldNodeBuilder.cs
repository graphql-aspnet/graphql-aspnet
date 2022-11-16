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
    /// A syntax node builder that builds field nodes from a token stream.
    /// </summary>
    internal class FieldNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new FieldNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="FieldNodeBuilder"/> class from being created.
        /// </summary>
        private FieldNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.Name);

            var startLocation = tokenStream.Location;
            var fieldName = tokenStream.ActiveToken.Block;
            var fieldAlias = fieldName;
            tokenStream.Next();

            // account for a possible alias on the field name
            if (tokenStream.Match(TokenType.Colon))
            {
                tokenStream.Next();
                tokenStream.MatchOrThrow(TokenType.Name);

                fieldName = tokenStream.ActiveToken.Block;
                tokenStream.Next();
            }

            var fieldNode = new SyntaxNode(
                SyntaxNodeType.Field,
                startLocation,
                new SyntaxNodeValue(fieldName),
                new SyntaxNodeValue(fieldAlias));

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref fieldNode);

            // account for possible collection of input values
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var builder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.InputItemCollection);
                builder.BuildNode(ref synTree, ref fieldNode, ref tokenStream);
            }

            // account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.Directive);

                do
                {
                    dirBuilder.BuildNode(ref synTree, ref fieldNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            // account for posible field collection on this field
            if (tokenStream.Match(TokenType.CurlyBraceLeft))
            {
                var fieldCollectionBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.FieldCollection);
                fieldCollectionBuilder.BuildNode(ref synTree, ref fieldNode, ref tokenStream);
            }
        }
    }
}
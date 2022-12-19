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
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;
    using KEYWORDS = GraphQL.AspNet.Execution.Parsing.ParserConstants.Keywords;

    /// <summary>
    /// A syntax node builder that builds named fragment nodes from a token stream.
    /// </summary>
    internal class NamedFragmentNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the single global instance of this builder.
        /// </summary>
        /// <value>The node builder instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new NamedFragmentNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="NamedFragmentNodeBuilder"/> class from being created.
        /// </summary>
        private NamedFragmentNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            // a root fragment must be in the form of keywords:  fragment on TargetType{}

            // "fragment" keyword
            var startLocation = tokenStream.Location;
            tokenStream.MatchOrThrow(KEYWORDS.Fragment.Span);
            tokenStream.Next();

            // name of the fragment
            tokenStream.MatchOrThrow(TokenType.Name);
            var fragmentName = tokenStream.ActiveToken.Block;
            tokenStream.Next();

            // "on" keyword
            // optional on parsing, check as rule 5.5.1.2 during validation
            var targetType = SourceTextBlockPointer.None;
            if (tokenStream.Match(KEYWORDS.On.Span))
            {
                tokenStream.Next();

                // target type
                tokenStream.MatchOrThrow(TokenType.Name);
                targetType = tokenStream.ActiveToken.Block;
                tokenStream.Next();
            }

            var namedFragmentNode = new SyntaxNode(
                SyntaxNodeType.NamedFragment,
                startLocation,
                new SyntaxNodeValue(fragmentName),
                new SyntaxNodeValue(targetType));

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref namedFragmentNode);

            // account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.Directive);

                do
                {
                    dirBuilder.BuildNode(ref synTree, ref namedFragmentNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            // must be pointing at the fragment field set now
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);

            var fieldCollectionBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.FieldCollection);
            fieldCollectionBuilder.BuildNode(ref synTree, ref namedFragmentNode, ref tokenStream);
        }
    }
}
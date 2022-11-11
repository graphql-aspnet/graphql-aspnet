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
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;
    using KEYWORDS = GraphQL.AspNet.Parsing2.ParserConstants.Keywords;

    public class NamedFragmentNodeBuilder : ISynNodeBuilder
    {
        public static ISynNodeBuilder Instance { get; } = new NamedFragmentNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="NamedFragmentNodeBuilder"/> class from being created.
        /// </summary>
        private NamedFragmentNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
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

            var namedFragmentNode = new SynNode(
                SynNodeType.NamedFragment,
                startLocation,
                new SynNodeValue(fragmentName),
                new SynNodeValue(targetType));

            SynTreeOperations.AddChildNode(ref synTree, ref parentNode, ref namedFragmentNode);

            // account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.Directive);

                do
                {
                    dirBuilder.BuildNode(ref synTree, ref namedFragmentNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            // must be pointing at the fragment field set now
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);

            var fieldCollectionBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.FieldCollection);
            fieldCollectionBuilder.BuildNode(ref synTree, ref namedFragmentNode, ref tokenStream);
        }
    }
}
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
    using System;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Exceptions;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;
    using KEYWORDS = GraphQL.AspNet.Parsing2.ParserConstants.Keywords;
    using GraphQL.AspNet.Parsing2.Lexing.Source;

    public class FragmentSpreadNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new FragmentSpreadNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="FragmentSpreadNodeBuilder"/> class from being created.
        /// </summary>
        private FragmentSpreadNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.SpreadOperator);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            SourceTextBlockPointer fragmentName = SourceTextBlockPointer.None;
            SourceTextBlockPointer restrictedToType = SourceTextBlockPointer.None;

            var inlineFragmentNode = new SynNode(
                SynNodeType.InlineFragment,
                startLocation);

            SynNode fragmentNode;
            SynNodeType nodeType = SynNodeType.InlineFragment;

            if (tokenStream.Match(KEYWORDS.On.Span))
            {
                // check for an optional type declaration (e.g. "on Type")
                // i.e. an inline fragment
                //
                // this is optional because an inline fragment does not have to
                // declare a type restriction
                tokenStream.Next();
                tokenStream.MatchOrThrow(TokenType.Name);
                restrictedToType = tokenStream.ActiveToken.Block;
                tokenStream.Next();
            }
            else if (tokenStream.Match(TokenType.Name))
            {
                // might be a named fragment spread (e.g.  ... myNamedFragment)
                nodeType = SynNodeType.FragmentSpread;
                fragmentName = tokenStream.ActiveToken.Block;
                tokenStream.Next();
            }

            // if niether of the two conditions matched its just a spread
            // with no restrictions
            if (nodeType == SynNodeType.FragmentSpread)
            {
                fragmentNode = new SynNode(
                    SynNodeType.FragmentSpread,
                    startLocation,
                    new SynNodeValue(fragmentName));
            }
            else
            {
                fragmentNode = new SynNode(
                    SynNodeType.InlineFragment,
                    startLocation,
                    new SynNodeValue(restrictedToType));
            }

            SynTreeOperations.AddChildNode(ref synTree, ref parentNode, ref fragmentNode);

            // account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.Directive);

                do
                {
                    dirBuilder.BuildNode(ref synTree, ref fragmentNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            if (tokenStream.Match(TokenType.CurlyBraceLeft))
            {
                // when a field set is declared then it must be an inline fragment
                // not a fragment spread (fragment spreads cannot declare fields)
                if (fragmentNode.NodeType == SynNodeType.FragmentSpread)
                {
                    throw new GraphQLSyntaxException(
                    startLocation,
                    "Invalid fragment syntax. A fragment spread may not declare a field set.");
                }

                var fieldCollectionBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.FieldCollection);
                fieldCollectionBuilder.BuildNode(ref synTree, ref fragmentNode, ref tokenStream);
            }
            else
            {
                // when no field set is declared it must be a spread, not an inline fragment
                if (fragmentNode.NodeType != SynNodeType.FragmentSpread)
                {
                    throw new GraphQLSyntaxException(
                    startLocation,
                    "Invalid fragment syntax. An inline fragment MUST declare a field set.");
                }
            }
        }
    }
}
namespace GraphQL.AspNet.Parsing2.NodeBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.NodeMakers.FieldMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    internal class FieldCollectionNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new FieldCollectionNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="FieldCollectionNodeBuilder"/> class from being created.
        /// </summary>
        private FieldCollectionNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            // the token stream MUST be positioned at an open curley brace for this to function
            // correclty
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            var fieldCollectionNode = new SynNode(
                SynNodeType.FieldCollection,
                startLocation);

            synTree = synTree.AddChildNode(ref parentNode, ref fieldCollectionNode);

            if (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.CurlyBraceRight))
            {
                do
                {
                    var builder = this.CreateFieldBuilder(ref tokenStream);
                    builder.BuildNode(ref synTree, ref fieldCollectionNode, ref tokenStream);
                }
                while (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.CurlyBraceRight));
            }

            // ensure and move past the close curly brace of the collection
            tokenStream.MatchOrThrow(TokenType.CurlyBraceRight);
            tokenStream.Next();
        }

        private ISynNodeBuilder CreateFieldBuilder(ref TokenStream tokenStream)
        {
            if (tokenStream.Match(TokenType.SpreadOperator))
                return FragmentSpreadNodeBuilder.Instance;
            else
                return FieldNodeBuilder.Instance;
        }
    }
}

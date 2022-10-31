namespace GraphQL.AspNet.Parsing2.NodeBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    public class InputItemCollectionNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new InputItemCollectionNodeBuilder();

        private InputItemCollectionNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            // parentleft:  input item collection on a field
            // curlybraceleft: input item collection in a complex field
            tokenStream.MatchOrThrow(TokenType.ParenLeft, TokenType.CurlyBraceLeft);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            if (!tokenStream.EndOfStream
                && !tokenStream.Match(TokenType.ParenRight, TokenType.CurlyBraceRight))
            {
                // only add the collection node if there is something put in it
                var inputItemCollectionNode = new SynNode(
                    SynNodeType.InputItemCollection,
                    startLocation);

                synTree = synTree.AddChildNode(ref parentNode, ref inputItemCollectionNode);

                var builder = NodeBuilderFactory.CreateBuilder(SynNodeType.InputItem);
                do
                {
                    builder.BuildNode(ref synTree, ref inputItemCollectionNode, ref tokenStream);
                }
                while (!tokenStream.EndOfStream
                    && !tokenStream.Match(TokenType.ParenRight, TokenType.CurlyBraceRight));
            }

            // ensure the paren right is being pointed at in the stream
            // then consume it
            tokenStream.MatchOrThrow(TokenType.ParenRight, TokenType.CurlyBraceRight);
            tokenStream.Next();
        }
    }
}
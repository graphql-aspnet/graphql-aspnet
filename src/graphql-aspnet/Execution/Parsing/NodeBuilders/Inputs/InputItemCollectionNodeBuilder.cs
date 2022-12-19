﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.NodeBuilders.Inputs
{
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;

    /// <summary>
    /// A syntax node builder that creates an input item collection (for a field or directive)
    /// from a token stream.
    /// </summary>
    internal class InputItemCollectionNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the instance of this builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new InputItemCollectionNodeBuilder();

        private InputItemCollectionNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
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
                var inputItemCollectionNode = new SyntaxNode(
                    SyntaxNodeType.InputItemCollection,
                    startLocation);

                SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref inputItemCollectionNode);

                var builder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.InputItem);
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
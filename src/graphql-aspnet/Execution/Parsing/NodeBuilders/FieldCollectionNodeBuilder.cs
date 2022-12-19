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

    /// <summary>
    /// A syntax node builder that builds field collection nodes from a token stream.
    /// </summary>
    internal class FieldCollectionNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new FieldCollectionNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="FieldCollectionNodeBuilder"/> class from being created.
        /// </summary>
        private FieldCollectionNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            // the token stream MUST be positioned at an open curley brace for this to function
            // correclty
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            var fieldCollectionNode = new SyntaxNode(
                SyntaxNodeType.FieldCollection,
                startLocation);

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref fieldCollectionNode);

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

        private ISyntaxNodeBuilder CreateFieldBuilder(ref TokenStream tokenStream)
        {
            if (tokenStream.Match(TokenType.SpreadOperator))
                return FragmentSpreadNodeBuilder.Instance;
            else
                return FieldNodeBuilder.Instance;
        }
    }
}
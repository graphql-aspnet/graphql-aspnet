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

    public class VariableCollectionNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new VariableCollectionNodeBuilder();

        private VariableCollectionNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            // the token stream MUST be positioned at an open paren for this maker to function correclty
            tokenStream.MatchOrThrow(TokenType.ParenLeft);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            if (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.ParenRight))
            {
                // only add the variable collection if there are variables within it
                var variableCollectionNode = new SyntaxNode(
                    SyntaxNodeType.VariableCollection,
                    startLocation);

                SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref variableCollectionNode);

                var variableBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.Variable);

                do
                {
                    variableBuilder.BuildNode(ref synTree, ref variableCollectionNode, ref tokenStream);
                }
                while (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.ParenRight));
            }

            // ensure and move past the close paren
            tokenStream.MatchOrThrow(TokenType.ParenRight);
            tokenStream.Next();
        }
    }
}
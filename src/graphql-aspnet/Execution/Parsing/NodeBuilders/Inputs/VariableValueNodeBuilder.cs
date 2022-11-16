// *************************************************************
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
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;

    public class VariableValueNodeBuilder : ISyntaxNodeBuilder
    {
        public static ISyntaxNodeBuilder Instance { get; } = new VariableValueNodeBuilder();

        private VariableValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.Dollar);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            tokenStream.MatchOrThrow(TokenType.Name);
            var name = tokenStream.ActiveToken.Block;
            tokenStream.Next();

            var variableValueNode = new SyntaxNode(
                SyntaxNodeType.VariableValue,
                startLocation,
                new SyntaxNodeValue(name));

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref variableValueNode);
        }
    }
}
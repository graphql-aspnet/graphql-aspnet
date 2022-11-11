// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.NodeBuilders.Inputs
{
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    public class VariableValueNodeBuilder : ISynNodeBuilder
    {
        public static ISynNodeBuilder Instance { get; } = new VariableValueNodeBuilder();

        private VariableValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.Dollar);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            tokenStream.MatchOrThrow(TokenType.Name);
            var name = tokenStream.ActiveToken.Block;
            tokenStream.Next();

            var variableValueNode = new SynNode(
                SynNodeType.VariableValue,
                startLocation,
                new SynNodeValue(name));

            SynTreeOperations.AddChildNode(ref synTree, ref parentNode, ref variableValueNode);
        }
    }
}
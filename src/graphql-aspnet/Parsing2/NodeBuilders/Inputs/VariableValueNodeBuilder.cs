namespace GraphQL.AspNet.Parsing2.NodeBuilders.Inputs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

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
            var name = tokenStream.ActiveToken.Text;
            tokenStream.Next();

            var variableValueNode = new SynNode(
                SynNodeType.VariableValue,
                startLocation,
                new SynNodeValue(name));

            synTree = synTree.AddChildNode(ref parentNode, ref variableValueNode);
        }
    }
}

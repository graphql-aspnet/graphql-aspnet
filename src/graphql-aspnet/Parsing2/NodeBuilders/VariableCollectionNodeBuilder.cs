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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    public class VariableCollectionNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new VariableCollectionNodeBuilder();

        private VariableCollectionNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            // the token stream MUST be positioned at an open paren for this maker to function correclty
            tokenStream.MatchOrThrow(TokenType.ParenLeft);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            if (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.ParenRight))
            {
                // only add the variable collection if there are variables within it
                var variableCollectionNode = new SynNode(
                    SynNodeType.VariableCollection,
                    startLocation);

                synTree = synTree.AddChildNode(ref parentNode, ref variableCollectionNode);

                var variableBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.Variable);

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
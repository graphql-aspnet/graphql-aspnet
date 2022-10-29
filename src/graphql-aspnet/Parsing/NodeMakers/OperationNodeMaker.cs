// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.NodeMakers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;

    /// <summary>
    /// A maker that can create a top level operation type node including its
    /// field selection, variables etc.
    /// spec: https://graphql.github.io/graphql-spec/October2021/#sec-Language.Operations .
    /// </summary>
    public class OperationNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the singleton instance of this node maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new OperationNodeMaker();

        private OperationNodeMaker()
        {
        }

        /// <summary>
        /// Processes the queue as far as it needs to to generate a fully qualiffied
        /// <see cref="SyntaxNode" /> based on its ruleset.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>LexicalToken.</returns>
        public SyntaxNode MakeNode(ISyntaxNodeList nodeList, ref TokenStream tokenStream)
        {
            var collectionId = nodeList.BeginTempCollection();

            // check to see if this is qualified operation root
            // default to "query" as per the specification if not
            tokenStream.Prime();
            var operationNode = this.CreateNode(ref tokenStream);

            // a variable collection will begin with an open paren
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var variableMaker = NodeMakerFactory.CreateMaker<VariableCollectionNode>();
                var node = variableMaker.MakeNode(nodeList, ref tokenStream);

                if (node.Children != null)
                    nodeList.AddNodeToTempCollection(collectionId, node);
            }

            // account for possible directives on this operation
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirMaker = NodeMakerFactory.CreateMaker<DirectiveNode>();

                do
                {
                    var directive = dirMaker.MakeNode(nodeList, ref tokenStream);
                    nodeList.AddNodeToTempCollection(collectionId, directive);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            // only thing left on the operaton root is the field selection
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);

            var maker = NodeMakerFactory.CreateMaker<FieldCollectionNode>();
            var fieldCollection = maker.MakeNode(nodeList, ref tokenStream);

            if (fieldCollection.Children != null)
                nodeList.AddNodeToTempCollection(collectionId, fieldCollection);

            operationNode.SetChildren(nodeList, collectionId);

            return operationNode;
        }

        /// <summary>
        /// Creates the top level operation root.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>OperationRootNode.</returns>
        private OperationNode CreateNode(ref TokenStream tokenStream)
        {
            var startLocation = tokenStream.Location;
            ReadOnlyMemory<char> firstName = ReadOnlyMemory<char>.Empty;
            ReadOnlyMemory<char> secondName = ReadOnlyMemory<char>.Empty;

            if (tokenStream.Match(TokenType.Name))
            {
                firstName = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            if (tokenStream.Match(TokenType.Name))
            {
                secondName = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            return new OperationNode(startLocation, firstName, secondName);
        }
    }
}
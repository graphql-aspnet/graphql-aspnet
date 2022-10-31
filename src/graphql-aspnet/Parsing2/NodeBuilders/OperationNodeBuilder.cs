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
    using System;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A node builder that will build out a top level operation
    /// from the token stream.
    /// </summary>
    public class OperationNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this node builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new OperationNodeBuilder();

        private OperationNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            // check to see if this is qualified operation root
            // default to "query" as per the specification if not
            tokenStream.Prime();
            var operationNode = this.CreateNode(ref tokenStream);

            synTree = synTree.AddChildNode(ref parentNode, ref operationNode);

            // a variable collection will begin with an open paren
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var variableCollectionBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.VariableCollection);
                variableCollectionBuilder.BuildNode(ref synTree, ref operationNode, ref tokenStream);
            }

            // account for possible directives on this operation
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirbuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.Directive);

                do
                {
                    dirbuilder.BuildNode(ref synTree, ref operationNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            //// only thing left on the operaton root is the field selection
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);

            var fieldCollectionBuilder = NodeBuilderFactory.CreateBuilder(SynNodeType.FieldCollection);
            fieldCollectionBuilder.BuildNode(ref synTree, ref operationNode, ref tokenStream);
        }

        /// <summary>
        /// Creates the top level operation root.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>OperationRootNode.</returns>
        private SynNode CreateNode(ref TokenStream tokenStream)
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

            return new SynNode(
                SynNodeType.Operation,
                startLocation,
                new SynNodeValue(firstName),
                new SynNodeValue(secondName));
        }
    }
}
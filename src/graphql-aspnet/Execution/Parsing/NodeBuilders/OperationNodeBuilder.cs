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
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A node builder that will build out a top level operation
    /// from the token stream.
    /// </summary>
    public class OperationNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this node builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new OperationNodeBuilder();

        private OperationNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            // check to see if this is qualified operation root
            // default to "query" as per the specification if not
            tokenStream.Prime();
            var operationNode = this.CreateNode(ref tokenStream);

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref operationNode);

            // a variable collection will begin with an open paren
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var variableCollectionBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.VariableCollection);
                variableCollectionBuilder.BuildNode(ref synTree, ref operationNode, ref tokenStream);
            }

            // account for possible directives on this operation
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirbuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.Directive);

                do
                {
                    dirbuilder.BuildNode(ref synTree, ref operationNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            //// only thing left on the operaton root is the field selection
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);

            var fieldCollectionBuilder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.FieldCollection);
            fieldCollectionBuilder.BuildNode(ref synTree, ref operationNode, ref tokenStream);
        }

        /// <summary>
        /// Creates the top level operation root.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>OperationRootNode.</returns>
        private SyntaxNode CreateNode(ref TokenStream tokenStream)
        {
            var startLocation = tokenStream.Location;
            SourceTextBlockPointer firstName = SourceTextBlockPointer.None;
            SourceTextBlockPointer secondName = SourceTextBlockPointer.None;

            if (tokenStream.Match(TokenType.Name))
            {
                firstName = tokenStream.ActiveToken.Block;
                tokenStream.Next();
            }

            if (tokenStream.Match(TokenType.Name))
            {
                secondName = tokenStream.ActiveToken.Block;
                tokenStream.Next();
            }

            return new SyntaxNode(
                SyntaxNodeType.Operation,
                startLocation,
                new SyntaxNodeValue(firstName),
                new SyntaxNodeValue(secondName));
        }
    }
}
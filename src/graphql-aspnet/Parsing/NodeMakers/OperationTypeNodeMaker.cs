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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;

    /// <summary>
    /// A maker that can create a top level operation type node including its
    /// field selection, variables etc.
    /// spec: https://graphql.github.io/graphql-spec/June2018/#sec-Language.Operations .
    /// </summary>
    public class OperationTypeNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the singleton instance of this node maker.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new OperationTypeNodeMaker();

        private OperationTypeNodeMaker()
        {
        }

        /// <summary>
        /// Processes the queue as far as it needs to to generate a fully qualiffied
        /// <see cref="SyntaxNode" /> based on its ruleset.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>LexicalToken.</returns>
        public SyntaxNode MakeNode(TokenStream tokenStream)
        {
            var directives = new List<SyntaxNode>();
            SyntaxNode variableCollection = null;
            SyntaxNode fieldCollection = null;

            // check to see if this is qualified operation root
            // default to "query" as per the specification if not
            tokenStream.Prime();
            var operationTypeNode = this.CreateNode(tokenStream);

            // a variable collection will begin with an open paren
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var variableMaker = NodeMakerFactory.CreateMaker<VariableCollectionNode>();
                variableCollection = variableMaker.MakeNode(tokenStream);
            }

            // account for possible directives on this operation
            while (tokenStream.Match(TokenType.AtSymbol))
            {
                var dirMaker = NodeMakerFactory.CreateMaker<DirectiveNode>();
                var directive = dirMaker.MakeNode(tokenStream);
                directives.Add(directive);
            }

            // only thing left on the operaton root is the field selection
            tokenStream.MatchOrThrow(TokenType.CurlyBraceLeft);
            var maker = NodeMakerFactory.CreateMaker<FieldCollectionNode>();
            fieldCollection = maker.MakeNode(tokenStream);

            if (variableCollection != null)
                operationTypeNode.AddChild(variableCollection);

            foreach (var directive in directives)
                operationTypeNode.AddChild(directive);

            if (fieldCollection != null && fieldCollection.Children.Any())
                operationTypeNode.AddChild(fieldCollection);

            return operationTypeNode;
        }

        /// <summary>
        /// Creates the top level operation root.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>OperationRootNode.</returns>
        private OperationTypeNode CreateNode(TokenStream tokenStream)
        {
            var startLocation = tokenStream.Location;
            ReadOnlyMemory<char> firstName = ReadOnlyMemory<char>.Empty;
            ReadOnlyMemory<char> secondName = ReadOnlyMemory<char>.Empty;

            if (tokenStream.Match<NameToken>())
            {
                firstName = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            if (tokenStream.Match<NameToken>())
            {
                secondName = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            return new OperationTypeNode(startLocation, firstName, secondName);
        }
    }
}
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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A node creator that acts on a variable declaration set to parse the variables named in
    /// the query/mutation and ensure their supplied parts are parsed from a token set.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class VariableNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new VariableNodeMaker();

        private VariableNodeMaker()
        {
        }

        /// <summary>
        /// Processes the queue as far as it needs to to generate a fully qualiffied
        /// <see cref="SyntaxNode" /> based on its ruleset.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>LexicalToken.</returns>
        public SyntaxNode MakeNode(ref TokenStream tokenStream)
        {
            // extracts a variable in the format of:    $name: declaredType [= defaultValue]

            // the token stream MUST be positioned at a dollar sign for this maker to function correclty
            tokenStream.MatchOrThrow(TokenType.Dollar);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            // must be a name token
            tokenStream.MatchOrThrow(TokenType.Name);
            var variableName = tokenStream.ActiveToken.Text;
            tokenStream.Next();

            // must be a colon
            tokenStream.MatchOrThrow(TokenType.Colon);
            tokenStream.Next();

            // extract the type expression while the tokens are of characters '[', ']', '!', {NameToken}
            LexicalToken startToken = default;
            LexicalToken endToken = default;
            while (
                tokenStream.Match(TokenType.BracketLeft) ||
                tokenStream.Match(TokenType.BracketRight) ||
                tokenStream.Match(TokenType.Bang) ||
                tokenStream.Match(TokenType.Name))
            {
                startToken = startToken.TokenType == TokenType.None
                    ? tokenStream.ActiveToken
                    : startToken;
                endToken = tokenStream.ActiveToken;
                tokenStream.Next();
            }

            var typeExpression = ReadOnlyMemory<char>.Empty;
            if (startToken.TokenType != TokenType.None)
            {
                typeExpression = tokenStream.SourceText.Slice(
                    startToken.Location.AbsoluteIndex,
                    endToken.Location.AbsoluteIndex + endToken.Text.Length - startToken.Location.AbsoluteIndex);
            }

            // could be an equal sign for a default value
            SyntaxNode defaultValue = null;
            if (tokenStream.Match(TokenType.EqualsSign))
            {
                tokenStream.Next();
                var maker = NodeMakerFactory.CreateMaker<InputValueNode>();
                defaultValue = maker.MakeNode(ref tokenStream);
            }

            // could be directives with the @ symbol
            List<SyntaxNode> directives = null;
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var maker = NodeMakerFactory.CreateMaker<DirectiveNode>();
                directives = new List<SyntaxNode>();

                do
                {
                    var directiveNode = maker.MakeNode(ref tokenStream);
                    directives.Add(directiveNode);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            var variable = new VariableNode(startLocation, variableName, typeExpression);

            if (defaultValue != null)
                variable.AddChild(defaultValue);

            variable.AddChildren(directives);

            return variable;
        }
    }
}
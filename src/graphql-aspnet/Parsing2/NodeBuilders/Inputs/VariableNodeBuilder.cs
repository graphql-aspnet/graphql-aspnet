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
    using System;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    public class VariableNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new VariableNodeBuilder();

        private VariableNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            // extracts a variable in the format of:    $name: declaredType [= defaultValue]

            // the token stream MUST be positioned at a dollar sign for this builder to function correclty
            tokenStream.MatchOrThrow(TokenType.Dollar);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            // must be a name token
            tokenStream.MatchOrThrow(TokenType.Name);
            var variableName = tokenStream.ActiveToken.Block;
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

            SourceTextBlockPointer typeExpression = SourceTextBlockPointer.None;
            if (startToken.TokenType != TokenType.None)
            {
                typeExpression = new SourceTextBlockPointer(
                    startToken.Location.AbsoluteIndex,
                    endToken.Location.AbsoluteIndex + endToken.Block.Length - startToken.Location.AbsoluteIndex);
            }

            var variableNode = new SynNode(
                SynNodeType.Variable,
                startLocation,
                new SynNodeValue(variableName),
                new SynNodeValue(typeExpression));

            synTree = synTree.AddChildNode(ref parentNode, ref variableNode);

            // could be an equal sign for a default value
            if (tokenStream.Match(TokenType.EqualsSign))
            {
                tokenStream.Next();
                var builder = NodeBuilderFactory.CreateBuilder(SynNodeType.InputValue);
                builder.BuildNode(ref synTree, ref variableNode, ref tokenStream);
            }

            // could be directives with the @ symbol
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var builder = NodeBuilderFactory.CreateBuilder(SynNodeType.Directive);

                do
                {
                    builder.BuildNode(ref synTree, ref variableNode, ref tokenStream);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }
        }
    }
}
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
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A syntax node builder that builds boolean scalar value nodes from a token stream.
    /// </summary>
    public class BooleanValueNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the instance of this builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new BooleanValueNodeBuilder();

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            SyntaxNode synNode = default;
            if (tokenStream.Match(TokenType.Null))
            {
                synNode = new SyntaxNode(
                    SyntaxNodeType.NullValue,
                    tokenStream.Location,
                    new SyntaxNodeValue(
                        tokenStream.ActiveToken.Block,
                        ScalarValueType.Boolean));
            }
            else
            {
                tokenStream.MatchOrThrow(TokenType.Name);
                if (tokenStream.Match(ParserConstants.Keywords.True.Span)
                    || tokenStream.Match(ParserConstants.Keywords.False.Span))
                {
                    synNode = new SyntaxNode(
                        SyntaxNodeType.ScalarValue,
                        tokenStream.Location,
                        new SyntaxNodeValue(
                            tokenStream.ActiveToken.Block,
                            ScalarValueType.Boolean));
                }
                else
                {
                    GraphQLSyntaxException.ThrowFromExpectation(
                        tokenStream.Location,
                        "{true|false}",
                        tokenStream.ActiveTokenText.ToString());
                }
            }

            tokenStream.Next();
            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref synNode);
        }
    }
}
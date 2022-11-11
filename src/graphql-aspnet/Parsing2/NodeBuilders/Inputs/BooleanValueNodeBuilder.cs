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
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Exceptions;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    public class BooleanValueNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance of this builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new BooleanValueNodeBuilder();

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            SynNode synNode = default;
            if (tokenStream.Match(TokenType.Null))
            {
                synNode = new SynNode(
                    SynNodeType.NullValue,
                    tokenStream.Location,
                    new SynNodeValue(
                        tokenStream.ActiveToken.Block,
                        ScalarValueType.Boolean));
            }
            else
            {
                tokenStream.MatchOrThrow(TokenType.Name);
                if (tokenStream.Match(ParserConstants.Keywords.True.Span)
                    || tokenStream.Match(ParserConstants.Keywords.False.Span))
                {
                    synNode = new SynNode(
                        SynNodeType.ScalarValue,
                        tokenStream.Location,
                        new SynNodeValue(
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
            SynTreeOperations.AddChildNode(ref synTree, ref parentNode, ref synNode);
        }
    }
}
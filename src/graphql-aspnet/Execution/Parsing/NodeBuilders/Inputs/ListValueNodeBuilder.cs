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

    public class ListValueNodeBuilder : ISyntaxNodeBuilder
    {
        public static ISyntaxNodeBuilder Instance { get; } = new ListValueNodeBuilder();

        private ListValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            var startLocation = tokenStream.Location;

            tokenStream.MatchOrThrow(TokenType.BracketLeft);
            tokenStream.Next();

            var listNode = new SyntaxNode(
                SyntaxNodeType.ListValue,
                startLocation);

            SyntaxTreeOperations.AddChildNode(ref synTree, ref parentNode, ref listNode);

            if (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.BracketRight))
            {
                do
                {
                    var childBuilder = ValueNodeBuilderFactory.CreateBuilder(tokenStream);
                    if (childBuilder == null)
                    {
                        throw new GraphQLSyntaxException(
                            tokenStream.Location,
                            $"Unexpected token in list, no value could be parsed. Expected '{TokenType.BracketRight.Description()}' or an " +
                            $"input object value but receieved '{tokenStream.ActiveToken.Block}'");
                    }

                    childBuilder.BuildNode(ref synTree, ref listNode, ref tokenStream);
                }
                while (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.BracketRight));
            }

            // ensure we are pointing at an end of list
            tokenStream.MatchOrThrow(TokenType.BracketRight);
            tokenStream.Next();
        }
    }
}
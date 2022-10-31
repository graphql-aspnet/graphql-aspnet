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
    using System.Collections.ObjectModel;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    public class ListValueNodeBuilder : ISynNodeBuilder
    {
        public static ISynNodeBuilder Instance { get; } = new ListValueNodeBuilder();

        private ListValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            var startLocation = tokenStream.Location;

            tokenStream.MatchOrThrow(TokenType.BracketLeft);
            tokenStream.Next();


            var listNode = new SynNode(
                SynNodeType.ListValue,
                startLocation);

            synTree = synTree.AddChildNode(ref parentNode, ref listNode);

            if (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.BracketRight))
            {
                do
                {
                    var childBuilder = ValueNodeBuilderFactory.CreateBuilder(tokenStream.ActiveToken);
                    if (childBuilder == null)
                    {
                        throw new GraphQLSyntaxException(
                            tokenStream.Location,
                            $"Unexpected token in list, no value could be parsed. Expected '{TokenType.BracketRight.Description()}' or an " +
                            $"input object value but receieved '{tokenStream.ActiveToken.Text}'");
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
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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    public class InputItemCollectionNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new InputItemCollectionNodeBuilder();

        private InputItemCollectionNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            // parentleft:  input item collection on a field
            // curlybraceleft: input item collection in a complex field
            tokenStream.MatchOrThrow(TokenType.ParenLeft, TokenType.CurlyBraceLeft);
            var startLocation = tokenStream.Location;
            tokenStream.Next();

            if (!tokenStream.EndOfStream
                && !tokenStream.Match(TokenType.ParenRight, TokenType.CurlyBraceRight))
            {
                // only add the collection node if there is something put in it
                var inputItemCollectionNode = new SynNode(
                    SynNodeType.InputItemCollection,
                    startLocation);

                SynTreeOperations.AddChildNode(ref synTree, ref parentNode, ref inputItemCollectionNode);

                var builder = NodeBuilderFactory.CreateBuilder(SynNodeType.InputItem);
                do
                {
                    builder.BuildNode(ref synTree, ref inputItemCollectionNode, ref tokenStream);
                }
                while (!tokenStream.EndOfStream
                    && !tokenStream.Match(TokenType.ParenRight, TokenType.CurlyBraceRight));
            }

            // ensure the paren right is being pointed at in the stream
            // then consume it
            tokenStream.MatchOrThrow(TokenType.ParenRight, TokenType.CurlyBraceRight);
            tokenStream.Next();
        }
    }
}
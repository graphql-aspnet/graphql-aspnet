// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.NodeMakers.FieldMakers
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// a node maker, expecting to start at a name token on a stream
    /// and will parse the stream in an attempt to extract a single, qualified field reference.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class FieldNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new FieldNodeMaker();

        private FieldNodeMaker()
        {
        }

        /// <inheritdoc />
        public SyntaxNode MakeNode(ISyntaxNodeList masterNodeList, ref TokenStream tokenStream)
        {
            tokenStream.MatchOrThrow(TokenType.Name);

            var startLocation = tokenStream.Location;
            var fieldName = tokenStream.ActiveToken.Text;
            var fieldAlias = fieldName;
            tokenStream.Next();

            var collectionId = masterNodeList.BeginTempCollection();

            // account for a possible alias on the field name
            if (tokenStream.Match(TokenType.Colon))
            {
                tokenStream.Next();
                tokenStream.MatchOrThrow(TokenType.Name);

                fieldName = tokenStream.ActiveToken.Text;
                tokenStream.Next();
            }

            // account for possible collection of input values
            if (tokenStream.Match(TokenType.ParenLeft))
            {
                var maker = NodeMakerFactory.CreateMaker<InputItemCollectionNode>();
                var inputCollection = maker.MakeNode(masterNodeList, ref tokenStream);
                if (inputCollection.Children != null)
                    masterNodeList.AddNodeToTempCollection(collectionId, inputCollection);
            }

            // account for possible directives on this field
            if (tokenStream.Match(TokenType.AtSymbol))
            {
                var maker = NodeMakerFactory.CreateMaker<DirectiveNode>();

                do
                {
                    var directive = maker.MakeNode(masterNodeList, ref tokenStream);
                    masterNodeList.AddNodeToTempCollection(collectionId, directive);
                }
                while (tokenStream.Match(TokenType.AtSymbol));
            }

            // account for posible field collection on this field
            if (tokenStream.Match(TokenType.CurlyBraceLeft))
            {
                var maker = NodeMakerFactory.CreateMaker<FieldCollectionNode>();
                var fieldCollection = maker.MakeNode(masterNodeList, ref tokenStream);
                if (fieldCollection.Children != null)
                    masterNodeList.AddNodeToTempCollection(collectionId, fieldCollection);
            }

            var node = new FieldNode(startLocation, fieldAlias, fieldName);
            node.SetChildren(masterNodeList, collectionId);
            return node;
        }
    }
}
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
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;

    /// <summary>
    /// A maker responsible for parsing the stream through a collection of top level
    /// variables. Expects the stream to point to an open paren and will process just past
    /// its matching close paren.
    /// </summary>
    /// <seealso cref="ISyntaxNodeMaker" />
    public class VariableCollectionNodeMaker : ISyntaxNodeMaker
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeMaker Instance { get; } = new VariableCollectionNodeMaker();

        private VariableCollectionNodeMaker()
        {
        }

        /// <summary>
        /// Processes the queue as far as it needs to to generate a fully qualiffied
        /// <see cref="SyntaxNode" /> based on its ruleset.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns>LexicalToken.</returns>
        public SyntaxNode MakeNode(ISyntaxNodeList nodeList, ref TokenStream tokenStream)
        {
            // the token stream MUST be positioned at an open paren for this maker to function correclty
            tokenStream.MatchOrThrow(TokenType.ParenLeft);

            var variableCollectionNode = new VariableCollectionNode(tokenStream.Location);
            tokenStream.Next();

            var collectionId = nodeList.BeginTempCollection();

            var variableMaker = NodeMakerFactory.CreateMaker<VariableNode>();
            while (!tokenStream.EndOfStream && !tokenStream.Match(TokenType.ParenRight))
            {
                var variable = variableMaker.MakeNode(nodeList, ref tokenStream);
                nodeList.AddNodeToTempCollection(collectionId, variable);
            }

            // ensure and move past the close paren
            tokenStream.MatchOrThrow(TokenType.ParenRight);
            tokenStream.Next();

            variableCollectionNode.SetChildren(nodeList, collectionId);
            return variableCollectionNode;
        }
    }
}
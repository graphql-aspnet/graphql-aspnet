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
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    internal class EnumValueNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the singleton instance of this builder.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new EnumValueNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="EnumValueNodeBuilder"/> class from being created.
        /// </summary>
        private EnumValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            if (tokenStream.Match(TokenType.Null))
            {
                ValueNodeBuilderFactory
                    .CreateBuilder(tokenStream)
                    .BuildNode(ref synTree, ref parentNode, ref tokenStream);
            }
            else
            {
                tokenStream.MatchOrThrow(TokenType.Name);

                var enumValueNode = new SynNode(
                    SynNodeType.EnumValue,
                    tokenStream.Location,
                    new SynNodeValue(
                        tokenStream.ActiveToken.Block));

                SynTreeOperations.AddChildNode(ref synTree, ref parentNode, ref enumValueNode);
                tokenStream.Next();
            }
        }
    }
}
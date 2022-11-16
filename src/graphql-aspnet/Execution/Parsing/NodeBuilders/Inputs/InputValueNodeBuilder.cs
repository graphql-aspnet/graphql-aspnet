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
    using GraphQL.AspNet.Parsing2.Exceptions;
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    internal class InputValueNodeBuilder : ISyntaxNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISyntaxNodeBuilder Instance { get; } = new InputValueNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="InputValueNodeBuilder"/> class from being created.
        /// </summary>
        private InputValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream)
        {
            var builder = ValueNodeBuilderFactory.CreateBuilder(tokenStream);
            if (builder != null)
            {
                builder.BuildNode(ref synTree, ref parentNode, ref tokenStream);
                return;
            }

            GraphQLSyntaxException.ThrowFromExpectation(
                tokenStream.Location,
                "<value>",
                tokenStream.TokenType.Description().ToString());

            throw new InvalidOperationException("Critical Failure, this exception should never be reached.");
        }
    }
}
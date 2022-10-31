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
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;

    internal class InputValueNodeBuilder : ISynNodeBuilder
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISynNodeBuilder Instance { get; } = new InputValueNodeBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="InputValueNodeBuilder"/> class from being created.
        /// </summary>
        private InputValueNodeBuilder()
        {
        }

        /// <inheritdoc />
        public void BuildNode(ref SynTree synTree, ref SynNode parentNode, ref TokenStream tokenStream)
        {
            var builder = ValueNodeBuilderFactory.CreateBuilder(tokenStream.ActiveToken);
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
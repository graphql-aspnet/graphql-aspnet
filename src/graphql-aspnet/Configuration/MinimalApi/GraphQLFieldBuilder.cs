// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.MinimalApi
{
    using GraphQL.AspNet.Interfaces.Configuration;

    internal class GraphQLFieldBuilder : BaseGraphQLFieldBuilder, IGraphQLFieldBuilder
    {
        public GraphQLFieldBuilder(
            SchemaOptions schemaOptions,
            string fullPathTemplate)
            : base(schemaOptions, fullPathTemplate)
        {
        }

        public GraphQLFieldBuilder(
            IGraphQLFieldGroupBuilder groupBuilder,
            string partialPathTemplate)
            : base(groupBuilder, partialPathTemplate)
        {
        }
    }
}
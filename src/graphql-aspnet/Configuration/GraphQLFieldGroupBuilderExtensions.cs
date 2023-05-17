// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using GraphQL.AspNet.Interfaces.Configuration;

    /// <summary>
    /// Extension methods for configuring specifical aspects of a field group generated via
    /// the minimal API
    /// </summary>
    public static class GraphQLFieldGroupBuilderExtensions
    {
        public static IGraphQLFieldBuilder RequireAuthorization(this IGraphQLFieldBuilder template)
        {
            return template;
        }
    }
}
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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An internal implementation of <see cref="IGraphQLFieldGroupBuilder"/>
    /// used as a base object during minimal api construction.
    /// </summary>
    internal class GraphQLFieldGroupBuilder : BaseGraphQLFieldBuilder, IGraphQLFieldGroupBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLFieldGroupBuilder"/> class.
        /// </summary>
        /// <param name="options">The schema options that will own the fields created from
        /// this builder.</param>
        /// <param name="groupTemplate">The partial field path template
        /// that will be prepended to any fields or groups created from this builder.</param>
        public GraphQLFieldGroupBuilder(
            SchemaOptions options,
            string groupTemplate)
            : base(options, groupTemplate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLFieldGroupBuilder"/> class.
        /// </summary>
        /// <param name="parentGroupBuilder">The parent group builder from which
        /// this builder will copy its settings.</param>
        /// <param name="groupSubTemplate">The partial path template to be appended to
        /// the parent's already defined template.</param>
        public GraphQLFieldGroupBuilder(IGraphQLFieldGroupBuilder parentGroupBuilder, string groupSubTemplate)
            : base(parentGroupBuilder, groupSubTemplate)
        {
        }
    }
}
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

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLFieldTemplate"/>
    /// used to generate new graphql fields via a minimal api style of coding.
    /// </summary>
    internal class GraphQLVirtualFieldTemplate : BaseGraphQLFieldTemplate, IGraphQLFieldTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLVirtualFieldTemplate"/> class.
        /// </summary>
        /// <param name="options">The schema options that will own the fields created from
        /// this builder.</param>
        /// <param name="fieldTemplate">The partial field path template
        /// that will be prepended to any fields or groups created from this builder.</param>
        public GraphQLVirtualFieldTemplate(
            SchemaOptions options,
            string fieldTemplate)
            : base(options, fieldTemplate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLVirtualFieldTemplate"/> class.
        /// </summary>
        /// <param name="parentFieldBuilder">The parent virtual field from which
        /// this builder will copy its settings.</param>
        /// <param name="fieldSubTemplate">The partial path template to be appended to
        /// the parent's already defined template.</param>
        public GraphQLVirtualFieldTemplate(IGraphQLFieldTemplate parentFieldBuilder, string fieldSubTemplate)
            : base(parentFieldBuilder, fieldSubTemplate)
        {
        }
    }
}
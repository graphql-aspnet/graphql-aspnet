// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions
{
    /// <summary>
    /// An intermediate template that utilizies a key/value pair system to build up a set of component parts
    /// that the templating engine will use to generate a full fledged field in a schema.
    /// </summary>
    public interface IGraphQLRuntimeFieldGroupDefinition : IGraphQLRuntimeSchemaItemDefinition
    {
        /// <summary>
        /// Creates a new, resolvable field as a child of this group instance.
        /// </summary>
        /// <param name="pathTemplate">The path template to incorporate on the field.</param>
        /// <returns>IGraphQLRuntimeResolvedFieldDefinition.</returns>
        IGraphQLRuntimeResolvedFieldDefinition MapField(string pathTemplate);

        /// <summary>
        /// Creates an intermediate child group, nested under this group instance with the given
        /// path template.
        /// </summary>
        /// <param name="pathTemplate">The path template to incorpate on the child group.</param>
        /// <returns>IGraphQLRuntimeFieldGroupDefinition.</returns>
        IGraphQLRuntimeFieldGroupDefinition MapChildGroup(string pathTemplate);
    }
}
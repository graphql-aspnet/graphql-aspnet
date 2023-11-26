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
    /// A field generated via this builder is identicial to a field parsed from a controller action
    /// in that it has an explicit resolver applied. The runtime will not attempt to
    /// autoresolve this field.
    /// </summary>
    public interface IGraphQLRuntimeResolvedFieldDefinition : IGraphQLRuntimeSchemaItemDefinition, IGraphQLResolvableSchemaItemDefinition
    {
    }
}
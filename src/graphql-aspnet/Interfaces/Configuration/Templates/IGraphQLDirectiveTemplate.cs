// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration.Templates
{
    /// <summary>
    /// An intermediate template that utilizies a key/value pair system to build up a set of component parts
    /// that the templating engine will use to generate a full fledged field in a schema.
    /// </summary>
    public interface IGraphQLDirectiveTemplate : IGraphQLRuntimeSchemaItemTemplate, IGraphQLResolvableSchemaItemTemplate
    {
    }
}
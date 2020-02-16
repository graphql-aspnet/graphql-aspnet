// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface representing an object acting as the runtime for the core graphql
    /// engine for a given schema. This runtime accepts requests and renders responses, nothing else.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this runtime exists for.</typeparam>
    public interface IGraphQLRuntime<TSchema> : IGraphQLRuntime
        where TSchema : class, ISchema
    {
    }
}
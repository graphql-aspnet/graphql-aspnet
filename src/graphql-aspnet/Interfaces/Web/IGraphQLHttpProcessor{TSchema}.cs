// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Web
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A processor that is created from a DI container by a runtime handler to handle the individual
    /// request.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public interface IGraphQLHttpProcessor<TSchema> : IGraphQLHttpProcessor
        where TSchema : class, ISchema
    {
    }
}
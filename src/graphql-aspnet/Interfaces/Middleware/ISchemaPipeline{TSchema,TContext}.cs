// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Middleware
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A schema targeted, context-specific pipeline that can be invoked on behalf of a target schema to
    /// perform some work.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this pipeline exists for.</typeparam>
    /// <typeparam name="TContext">The type of the context the pipeline can process.</typeparam>
    public interface ISchemaPipeline<TSchema, TContext> : ISchemaPipeline<TContext>
        where TSchema : class, ISchema
        where TContext : class, IGraphMiddlewareContext
    {
    }
}
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder for performing advanced configuration of a schema's pipeline and processing settings.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this builder is creating the pipeline for.</typeparam>
    /// <typeparam name="TMiddleware">The type of middleware supported by the pipeline.</typeparam>
    /// <typeparam name="TContext">The type of the context the middleware components can handle.</typeparam>
    public interface ISchemaPipelineBuilder<TSchema, in TMiddleware, TContext>
        where TSchema : class, ISchema
        where TMiddleware : class, IGraphMiddlewareComponent<TContext>
        where TContext : class, IGraphExecutionContext
    {
        /// <summary>
        /// Adds the given type as a middleware component for the pipeline executed for this schema type. The middleware component
        /// will be added to the DI collection and created from it.
        /// </summary>
        /// <param name="middlewareInstance">The middleware instance to add to the pipeline.</param>
        /// <param name="name">A friendly, internal name to assign to this component. It will be referenced in any names or log messages that are generated.</param>
        /// <returns>ISchemaConfigurationBuilder.</returns>
        ISchemaPipelineBuilder<TSchema, TMiddleware, TContext> AddMiddleware(TMiddleware middlewareInstance, string name = null);

        /// <summary>
        /// Adds the given Func as a middleware component for this pipeline.
        /// </summary>
        /// <param name="operation"><para>The function to execute in the request pipeline.</para>
        /// <para>Method signature is: The invocation context context, The next delegate in the chain, The cancelation token | Returns a task.</para></param>
        /// <param name="name">A friendly, internal name to assign to this component. It will be referenced in any names or log messages that are generated.</param>
        /// <returns>ISchemaConfigurationBuilder.</returns>
        ISchemaPipelineBuilder<TSchema, TMiddleware, TContext> AddMiddleware(Func<TContext, GraphMiddlewareInvocationDelegate<TContext>, CancellationToken, Task> operation, string name = null);

        /// <summary>
        /// Adds the given type as a middleware component for this pipeline.
        /// </summary>
        /// <typeparam name="TComponent">The type of the middlware component.</typeparam>
        /// <param name="lifetime">The life time of the component will determine how often it is recreated or reused
        /// over mulitple invocations..</param>
        /// <param name="name">A friendly, internal name to assign to this component. It will be referenced in any names or log messages that are generated.</param>
        /// <returns>ISchemaConfigurationBuilder.</returns>
        ISchemaPipelineBuilder<TSchema, TMiddleware, TContext> AddMiddleware<TComponent>(
            ServiceLifetime lifetime = ServiceLifetime.Singleton,
            string name = null)
            where TComponent : class, TMiddleware;

        /// <summary>
        /// Clears this pipeline of any registered middleware items.
        /// </summary>
        /// <returns>ISchemaPipelineBuilder&lt;TMiddleware, TContext&gt;.</returns>
        ISchemaPipelineBuilder<TSchema, TMiddleware, TContext> Clear();

        /// <summary>
        /// Builds out the pipeline, in context of the schema, using the components defined on this builder.
        /// </summary>
        /// <returns>IGraphPipeline&lt;TContext&gt;.</returns>
        ISchemaPipeline<TSchema, TContext> Build();

        /// <summary>
        /// Gets the count of components in this pipeline.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }
    }
}
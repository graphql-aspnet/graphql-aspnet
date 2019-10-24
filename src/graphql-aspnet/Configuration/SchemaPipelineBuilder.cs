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
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Middleware.Common;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An builder class that can construct a schema pipeline for the given middleware type and context.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this pipeline builder is creating a pipelien for.</typeparam>
    /// <typeparam name="TMiddleware">The type of middleware supported by the pipeline.</typeparam>
    /// <typeparam name="TContext">The type of the context the middleware components can handle.</typeparam>
    public class SchemaPipelineBuilder<TSchema, TMiddleware, TContext> : ISchemaPipelineBuilder<TSchema, TMiddleware, TContext>
        where TSchema : class, ISchema
        where TMiddleware : class, IGraphMiddlewareComponent<TContext>
        where TContext : class, IGraphMiddlewareContext
    {
        /// <summary>
        /// Occurs when a middleware type reference is added to the pipeline.
        /// </summary>
        public event EventHandler<TypeReferenceEventArgs> TypeReferenceAdded;

        private readonly LinkedList<GraphMiddlewareDefinition<TContext>> _middleware;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaPipelineBuilder{TSchema,TMiddleware,TContext}" /> class.
        /// </summary>
        /// <param name="name">The human friendly name to assign to this pipeline.</param>
        public SchemaPipelineBuilder(string name = null)
        {
            this.PipelineName = name?.Trim() ?? "-unknown-";
            _middleware = new LinkedList<GraphMiddlewareDefinition<TContext>>();
        }

        /// <summary>
        /// Adds the given type as a middleware component for the pipeline executed for this schema type. The middleware component
        /// will be added to the DI collection and created from it.
        /// </summary>
        /// <param name="middlewareInstance">The middleware instance to add to the pipeline.</param>
        /// <param name="name">A friendly, internal name to assign to this component. It will be referenced in any names or log messages that are generated.</param>
        /// <returns>ISchemaConfigurationBuilder.</returns>
        public ISchemaPipelineBuilder<TSchema, TMiddleware, TContext> AddMiddleware(TMiddleware middlewareInstance, string name = null)
        {
            var definition = new GraphMiddlewareDefinition<TContext>(middlewareInstance, name);
            _middleware.AddLast(definition);
            return this;
        }

        /// <summary>
        /// Adds the given Func as a middleware component for this pipeline.
        /// </summary>
        /// <param name="operation"><para>The function to execute in the request pipeline.</para>
        /// <para>Method signature is: The invocation context context, The next delegate in the chain, The cancelation token | Returns a task.</para></param>
        /// <param name="name">A friendly, internal name to assign to this component. It will be referenced in any names or log messages that are generated.</param>
        /// <returns>ISchemaConfigurationBuilder.</returns>
        public ISchemaPipelineBuilder<TSchema, TMiddleware, TContext> AddMiddleware(
            Func<TContext, GraphMiddlewareInvocationDelegate<TContext>, CancellationToken, Task> operation,
            string name = null)
        {
            var middleware = new SingleFunctionMiddleware<TContext>(operation);
            var definition = new GraphMiddlewareDefinition<TContext>(middleware, name);
            _middleware.AddLast(definition);
            return this;
        }

        /// <summary>
        /// Adds the given type as a middleware component for this pipeline.
        /// </summary>
        /// <typeparam name="TComponent">The type of the middlware component.</typeparam>
        /// <param name="lifetime">The life time of the component will determine how often it is recreated or reused
        /// over mulitple invocations..</param>
        /// <param name="name">A friendly, internal name to assign to this component. It will be referenced in any names or log messages that are generated.</param>
        /// <returns>ISchemaConfigurationBuilder.</returns>
        public ISchemaPipelineBuilder<TSchema, TMiddleware, TContext> AddMiddleware<TComponent>(
            ServiceLifetime lifetime = ServiceLifetime.Singleton,
            string name = null)
            where TComponent : class, TMiddleware
        {
            var definition = new GraphMiddlewareDefinition<TContext>(typeof(TComponent), lifetime, name);
            _middleware.AddLast(definition);
            this.TypeReferenceAdded?.Invoke(this, new TypeReferenceEventArgs(typeof(TComponent), lifetime));

            return this;
        }

        /// <summary>
        /// Clears this pipeline of any registered middleware items.
        /// </summary>
        /// <returns>ISchemaPipelineBuilder&lt;TMiddleware, TContext&gt;.</returns>
        public ISchemaPipelineBuilder<TSchema, TMiddleware, TContext> Clear()
        {
            _middleware.Clear();
            return this;
        }

        /// <summary>
        /// Builds out the pipeline, in context of the schema, using the components added to this builder.
        /// </summary>
        /// <returns>IGraphPipeline&lt;TContext&gt;.</returns>
        public ISchemaPipeline<TSchema, TContext> Build()
        {
            GraphMiddlewareInvocationDelegate<TContext> leadInvoker = null;

            // walk backwards up the chained middleware, setting up the component creators and their call chain
            // maintain a list of component names for logging
            var node = _middleware.Last;
            var middlewareNameList = new List<string>();
            while (node != null)
            {
                var invoker = new GraphMiddlewareInvoker<TContext>(node.Value, leadInvoker);

                if (!string.IsNullOrWhiteSpace(node.Value?.Name))
                    middlewareNameList.Insert(0, node.Value.Name);
                else if (node.Value.Component != null)
                    middlewareNameList.Insert(0, node.Value.Component.GetType().FriendlyName());
                else if (node.Value.MiddlewareType != null)
                    middlewareNameList.Insert(0, node.Value.MiddlewareType.FriendlyName());
                else
                    middlewareNameList.Insert(0, "-unknown-");

                node = node.Previous;
                leadInvoker = invoker.InvokeAsync;
            }

            return new GraphSchemaPipeline<TSchema, TContext>(leadInvoker, this.PipelineName, middlewareNameList);
        }

        /// <summary>
        /// Gets the count of components in this pipeline.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _middleware.Count;

        /// <summary>
        /// Gets or sets the name to be assigned to the pipeline when its generated.
        /// </summary>
        /// <value>The name of the pipeline.</value>
        public string PipelineName { get; set; }
    }
}
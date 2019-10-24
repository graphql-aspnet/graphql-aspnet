// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A pipeline capable of processing a given context type for a schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this pipeline exists for.</typeparam>
    /// <typeparam name="TContext">The type of the middleware context processed by this pipeline.</typeparam>
    public class GraphSchemaPipeline<TSchema, TContext> : ISchemaPipeline<TSchema, TContext>
        where TSchema : class, ISchema
        where TContext : class, IGraphMiddlewareContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaPipeline{TSchema, TContext}" /> class.
        /// </summary>
        /// <param name="leadInvoker">The lead invoker.</param>
        /// <param name="pipelineName">The friendly name of this pipeline.</param>
        /// <param name="nameList">The name list.</param>
        public GraphSchemaPipeline(GraphMiddlewareInvocationDelegate<TContext> leadInvoker, string pipelineName, IReadOnlyList<string> nameList)
        {
            this.InvokeAsync = Validation.ThrowIfNullOrReturn(leadInvoker, nameof(leadInvoker));
            this.MiddlewareComponentNames = nameList ?? new List<string>();
            this.Name = pipelineName;
        }

        /// <summary>
        /// Gets the delegate function representing the start of the pipeline.
        /// </summary>
        /// <value>The delegate.</value>
        public GraphMiddlewareInvocationDelegate<TContext> InvokeAsync { get; }

        /// <summary>
        /// Gets a list, in execution order, of the middleware component names that are
        /// registered to this pipeline.
        /// </summary>
        /// <value>The middleware component names.</value>
        public IReadOnlyList<string> MiddlewareComponentNames { get; }

        /// <summary>
        /// Gets the friendly name of this pipeline instance.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }
    }
}
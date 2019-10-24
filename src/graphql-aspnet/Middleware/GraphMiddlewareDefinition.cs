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
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Middleware;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An internal definition of a piece of middleware sitting on a schema pipeline.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    [DebuggerDisplay("Middleware '{Name}'")]
    public class GraphMiddlewareDefinition<TContext>
        where TContext : class, IGraphMiddlewareContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMiddlewareDefinition{TMiddlewareComponent}" /> class.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="name">A friendly name to assign to this middleware component for easy reference.</param>
        public GraphMiddlewareDefinition(IGraphMiddlewareComponent<TContext> component, string name = null)
        {
            this.Component = Validation.ThrowIfNullOrReturn(component, nameof(component));
            this.Name = name?.Trim() ?? component.GetType().FriendlyName();
            this.Lifetime = ServiceLifetime.Singleton;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMiddlewareDefinition{TMiddlewareComponent}" /> class.
        /// </summary>
        /// <param name="middlewareType">Type of the middleware.</param>
        /// <param name="lifetime">The life time.</param>
        /// <param name="name">A friendly name to assign to this middleware component for easy reference.</param>
        public GraphMiddlewareDefinition(Type middlewareType, ServiceLifetime lifetime, string name = null)
        {
            Validation.ThrowIfNull(middlewareType, nameof(middlewareType));
            Validation.ThrowIfNotCastable<IGraphMiddlewareComponent<TContext>>(middlewareType, nameof(middlewareType));

            this.MiddlewareType = middlewareType;
            this.Lifetime = lifetime;
            this.Name = name?.Trim() ?? this.MiddlewareType.FriendlyName();
        }

        /// <summary>
        /// Gets the singular, reusable item that is a pipeline component. If supplied, <see cref="MiddlewareType"/> and <see cref="Lifetime"/>
        /// are ignored and the DI service provide will not be used to create this component. All <see cref="Func{T}"/> components
        /// make use of this.
        /// </summary>
        /// <value>The component.</value>
        public IGraphMiddlewareComponent<TContext> Component { get; }

        /// <summary>
        /// Gets the type of the middleware to be created.
        /// </summary>
        /// <value>The type of the middleware.</value>
        public Type MiddlewareType { get; }

        /// <summary>
        /// Gets the life time scope to be applied to the middleware at runtime.
        /// </summary>
        /// <value>The life time.</value>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Gets the friendly name assigned to describe this component.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }
    }
}
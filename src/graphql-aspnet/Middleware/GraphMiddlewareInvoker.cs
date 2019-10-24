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
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware.Exceptions;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An encapulated piece of middleware, defined by an end user, with all the
    /// technical bits to process the resultant pipeline correctly.
    /// </summary>
    /// <typeparam name="TContext">The type of the context the middleware component will accept on its invoke method.</typeparam>
    [DebuggerDisplay("Middleware Invoker '{ComponentDefinition.Name}'")]
    public class GraphMiddlewareInvoker<TContext>
        where TContext : class, IGraphMiddlewareContext
    {
        private readonly object _locker = new object();
        private IGraphMiddlewareComponent<TContext> _singletonInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMiddlewareInvoker{TContext}" /> class.
        /// </summary>
        /// <param name="middlewareComponent">The middleware component.</param>
        /// <param name="next">The next.</param>
        public GraphMiddlewareInvoker(
            GraphMiddlewareDefinition<TContext> middlewareComponent,
            GraphMiddlewareInvocationDelegate<TContext> next = null)
        {
            this.ComponentDefinition = Validation.ThrowIfNullOrReturn(middlewareComponent, nameof(middlewareComponent));
            _singletonInstance = this.ComponentDefinition.Component;
            this.Next = next;
            if (this.Next == null)
                this.Next = (_, __) => Task.CompletedTask;
        }

        /// <summary>
        /// Invokes the middleware instance in the manner provided by the definition.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        [DebuggerStepThrough]
        public async Task InvokeAsync(TContext context, CancellationToken cancelToken)
        {
            // stop processing when cancelation is requested
            if (cancelToken.IsCancellationRequested)
                return;

            // create and use the component, depending on the scope
            // invoke it and return
            var instance = _singletonInstance;
            if (instance == null)
            {
                // no pre-instantiated component was provided
                // generate the compoennt from the current context. If it was marked as a singleton scope
                // store the reference for use on subsequent calls
                instance = context?.ServiceProvider?
                    .GetService(this.ComponentDefinition.MiddlewareType) as IGraphMiddlewareComponent<TContext>;

                if (this.ComponentDefinition.Lifetime == ServiceLifetime.Singleton)
                {
                    lock (_locker)
                    {
                        if (_singletonInstance == null)
                            _singletonInstance = instance;
                    }
                }
            }

            if (instance == null)
            {
                throw new GraphPipelineMiddlewareInvocationException(
                    this.ComponentDefinition.Name,
                    $"Unable to resolve an instance of the middleware compoent '{this.ComponentDefinition.Name}'. Either no instance was provided " +
                    "or one could not be created from the service provider on the request context.");
            }

            var task = instance.InvokeAsync(context, this.Next, cancelToken);
            await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the next delegate to call in the request chain.
        /// </summary>
        /// <value>The next.</value>
        public GraphMiddlewareInvocationDelegate<TContext> Next { get; }

        /// <summary>
        /// Gets the component definition that is to invoked.
        /// </summary>
        /// <value>The component.</value>
        public GraphMiddlewareDefinition<TContext> ComponentDefinition { get; }
    }
}
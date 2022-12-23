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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware.Exceptions;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An encapulated piece of middleware with all the
    /// technical bits to correclty invoke the component as part of a pipeline.
    /// </summary>
    /// <typeparam name="TContext">The type of the context the middleware component will accept on its invoke method.</typeparam>
    [DebuggerDisplay("Middleware Invoker '{ComponentDefinition.Name}'")]
    internal class GraphMiddlewareInvoker<TContext>
        where TContext : class, IExecutionContext
    {
        private readonly object _locker = new object();
        private IGraphMiddlewareComponent<TContext> _singletonInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMiddlewareInvoker{TContext}" /> class.
        /// </summary>
        /// <param name="middlewareComponent">The middleware component being invoked.</param>
        /// <param name="next">A delegate pointing to the next component to invoke, if any.</param>
        public GraphMiddlewareInvoker(
            GraphMiddlewareDefinition<TContext> middlewareComponent,
            GraphMiddlewareInvocationDelegate<TContext> next = null)
        {
            this.ComponentDefinition = Validation.ThrowIfNullOrReturn(middlewareComponent, nameof(middlewareComponent));
            _singletonInstance = this.ComponentDefinition.Component;

            this.Next = next;
            if (this.Next == null)
                this.Next = (_, _) => Task.CompletedTask;
        }

        /// <summary>
        /// Invokes the middleware instance in the manner provided by the definition.
        /// </summary>
        /// <param name="context">The context being processed by this middleware component.</param>
        /// <param name="cancelToken">The cancel token governing the process. If the token has
        /// signaled a cancellation the middleware component will NOT be invoked.</param>
        /// <returns>Task.</returns>
        [DebuggerStepperBoundary]
        public async Task InvokeAsync(TContext context, CancellationToken cancelToken)
        {
            // stop processing when cancelation is requested
            if (cancelToken.IsCancellationRequested)
                return;

            // create and use the component, depending on the scope
            var instance = _singletonInstance;
            if (instance == null)
            {
                // no pre-instantiated component was provided at startup so
                // we need to generate the compoennt from the current context.
                // If the compoent is generated as a singleton
                // store it for use on subsequent calls
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

                if (instance == null)
                {
                    throw new GraphPipelineMiddlewareInvocationException(
                        this.ComponentDefinition.Name,
                        $"Unable to resolve an instance of the middleware compoent '{this.ComponentDefinition.Name}'. Either no instance was provided " +
                        "or one could not be created from the service provider on the request context.");
                }
            }

            // invoke the middleware component
            await instance.InvokeAsync(context, this.Next, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the next delegate to call in the request chain.
        /// </summary>
        /// <value>The next.</value>
        public GraphMiddlewareInvocationDelegate<TContext> Next { get; }

        /// <summary>
        /// Gets the component definition that is to invoked.
        /// </summary>
        /// <value>The original component definition that declares
        /// how this invoker should function.</value>
        public GraphMiddlewareDefinition<TContext> ComponentDefinition { get; }
    }
}
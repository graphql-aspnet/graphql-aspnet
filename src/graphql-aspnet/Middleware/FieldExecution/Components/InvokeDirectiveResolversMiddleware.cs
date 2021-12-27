﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.FieldExecution.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// A piece of middleware that will invoke any directives required for the current context.
    /// </summary>
    public class InvokeDirectiveResolversMiddleware : IGraphFieldExecutionMiddleware
    {
        /// <summary>
        /// Called by the runtime to invoke the middleware logic. In most cases, the middleware component should return the task
        /// generated by calling the next delegate in the request chain with the provided context.
        /// </summary>
        /// <param name="context">The invocation request governing data in this pipeline run.</param>
        /// <param name="next">The next delegate in the chain to be invoked.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(GraphFieldExecutionContext context, GraphMiddlewareInvocationDelegate<GraphFieldExecutionContext> next, CancellationToken cancelToken)
        {
            IEnumerable<IDirectiveInvocationContext> directives = context.Request.InvocationContext.Directives ?? Enumerable.Empty<IDirectiveInvocationContext>();

            if (context.IsValid)
            {
                // generate requests contexts for each directive to be processed
                bool cancelPipeline = false;
                foreach (var directive in directives)
                {
                    var directiveArguments = directive.Arguments.Merge(context.VariableData);
                    var request = new GraphDirectiveRequest(
                        directive.Directive,
                        directive.Location,
                        directive.Origin,
                        context.Request.Items);

                    var beforeResolutionRequest = request.ForLifeCycle(
                        DirectiveLifeCycleEvent.BeforeResolution,
                        context.Request.DataSource);

                    var directiveContext = new DirectiveResolutionContext(
                        context,
                        beforeResolutionRequest,
                        directiveArguments,
                        context.User);

                    await this.ExecuteDirective(directiveContext, cancelToken).ConfigureAwait(false);
                    context.Messages.AddRange(directiveContext.Messages);
                    cancelPipeline = cancelPipeline || directiveContext.IsCancelled;
                }

                if (cancelPipeline)
                {
                    this.CancelPipeline(context);
                }
            }

            // ---------------------------------
            // continue the pipeline
            // ---------------------------------
            await next(context, cancelToken).ConfigureAwait(false);

            // ---------------------------------
            // execute after completion directives
            // ---------------------------------
            if (context.IsValid)
            {
                bool cancelPipeline = false;
                foreach (var directive in directives)
                {
                    var directiveArguments = directive.Arguments.Merge(context.VariableData);
                    var request = new GraphDirectiveRequest(
                        directive.Directive,
                        directive.Location,
                        directive.Origin,
                        context.Request.Items);

                    var afterResolutionRequest = request.ForLifeCycle(
                        DirectiveLifeCycleEvent.AfterResolution,
                        context.Request.DataSource);

                    var directiveContext = new DirectiveResolutionContext(
                        context,
                        afterResolutionRequest,
                        directiveArguments);

                    await this.ExecuteDirective(directiveContext, cancelToken).ConfigureAwait(false);

                    context.Messages.AddRange(directiveContext.Messages);

                    cancelPipeline = cancelPipeline || directiveContext.IsCancelled;
                }

                if (cancelPipeline)
                {
                    this.CancelPipeline(context);
                }
            }
        }

        private void CancelPipeline(GraphFieldExecutionContext context)
        {
            context.Cancel();
            context.Request.DataSource.Items.ForEach(x => x.Cancel());
        }

        /// <summary>
        /// Executes the directive context allowing hte resolver to process the request and response dictated by their operations.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>An instruction on how to proceed after.</returns>
        private Task ExecuteDirective(
            DirectiveResolutionContext context,
            CancellationToken cancelToken)
        {
            return context.Request.Directive.Resolver.Resolve(context, cancelToken);
        }
    }
}
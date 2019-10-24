// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Resolvers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// Wraps a user provided function as a custom resolver to any declared field. Useful for small, mostly static
    /// resolution setups and ignores much of the passed data available to fully qualified resolvers.
    /// </summary>
    /// <typeparam name="TSource">The expected type of the source data.</typeparam>
    /// <typeparam name="TReturn">The expected type of the returned data.</typeparam>
    public class GraphDataValueResolver<TSource, TReturn> : IGraphFieldResolver
        where TSource : class
    {
        private readonly Func<TSource, Task<TReturn>> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDataValueResolver{TSource,TReturn}" /> class.
        /// </summary>
        /// <param name="func">The function to execute to resolve the field.</param>
        public GraphDataValueResolver(Func<TSource, Task<TReturn>> func)
        {
            _func = Validation.ThrowIfNullOrReturn(func, nameof(func));
        }

        /// <summary>
        /// Processes the given <see cref="IGraphFieldRequest" /> against this instance
        /// performing the operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="context">The field context containing the necessary data to resolve
        /// the field and produce a reslt.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public async Task Resolve(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            var data = await _func(context?.Arguments.SourceData as TSource).ConfigureAwait(false);
            context.Result = data;
        }

        /// <summary>
        /// Gets the concrete type this resolver attempts to create as a during its invocation.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ObjectType => typeof(TReturn);

        /// <summary>
        /// Gets the resolution mode the execution engine should use when invoking this resolver.
        /// </summary>
        /// <value>The mode.</value>
        public FieldResolutionMode Mode => FieldResolutionMode.PerSourceItem;
    }
}
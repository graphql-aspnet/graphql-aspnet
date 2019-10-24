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
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A delegate pointing to the invocation of a middleware component.
    /// </summary>
    /// <typeparam name="TContext">The type of the context the processed by the pipeline.</typeparam>
    /// <param name="context">The execution context containing the request being processed.</param>
    /// <param name="cancelToken">The cancel token.</param>
    /// <returns>Task.</returns>
    public delegate Task GraphMiddlewareInvocationDelegate<TContext>(TContext context, CancellationToken cancelToken);
}
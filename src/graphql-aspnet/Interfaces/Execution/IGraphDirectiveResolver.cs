// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A resolver that can process requests to invoke a directive and produce a result.
    /// </summary>
    public interface IGraphDirectiveResolver
    {
        /// <summary>
        /// Processes the given <see cref="IGraphDirectiveRequest" /> against this instance
        /// performing the operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="directiveRequest">The request context to be processed.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task.</returns>
        Task ResolveAsync(DirectiveResolutionContext directiveRequest, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the metadata set that describes this instance's implementation in the source code. The resolver will
        /// use this data to properly instantiate and invoke the configured object methods, Func, dynamic delegates or properties
        /// as appropriate.
        /// </summary>
        /// <value>The resolver's metadata collection.</value>
        IReadOnlyDictionary<DirectiveLocation, IGraphFieldResolverMetaData> MetaData { get; }
    }
}
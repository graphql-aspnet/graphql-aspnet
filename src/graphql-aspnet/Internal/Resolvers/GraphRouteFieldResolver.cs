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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A resolver that fulfills its requirements by taking no action. It serves as a stub for an intermediate field
    /// usually created during the creation of a nested field.
    /// </summary>
    /// <seealso cref="IGraphFieldResolver" />
    public class GraphRouteFieldResolver : IGraphFieldResolver
    {
        private readonly object _dataObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRouteFieldResolver" /> class.
        /// </summary>
        /// <param name="dataObject">The data object instance to return as the "result" of resolving this field. If not supplied a
        /// new instance of <see cref="object"/> will be returned.</param>
        public GraphRouteFieldResolver(object dataObject = null)
        {
            _dataObject = dataObject;
        }

        /// <summary>
        /// Processes the given <see cref="IGraphFieldRequest" /> against this instance
        /// performing the operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="context">The field context containing the necessary data to resolve
        /// the field and produce a reslt.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public Task Resolve(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            context.Result = _dataObject ?? new object();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the concrete type this resolver attempts to create during its operation.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ObjectType => typeof(object);

        /// <summary>
        /// Gets a value indicating whether this instance is a leaf field; one capable of generating
        /// a real data item vs. generating data to be used in down stream projections.
        /// </summary>
        /// <value><c>true</c> if this instance is a leaf field; otherwise, <c>false</c>.</value>
        public bool IsLeaf => false;
    }
}
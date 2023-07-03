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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A special resolver for resolving intermediate fields created from controller action
    /// path templates.
    /// </summary>
    /// <seealso cref="IGraphFieldResolver" />
    internal class GraphControllerRouteFieldResolver : IGraphFieldResolver
    {
        private readonly VirtualResolvedObject _dataObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphControllerRouteFieldResolver" /> class.
        /// </summary>
        /// <param name="dataObject">The data object instance to return as the "result" of resolving this field. If not supplied a
        /// new instance of <see cref="object"/> will be returned.</param>
        public GraphControllerRouteFieldResolver(VirtualResolvedObject dataObject)
        {
            Validation.ThrowIfNull(dataObject, nameof(dataObject));
            _dataObject = dataObject;
        }

        /// <inheritdoc />
        public Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            context.Result = _dataObject;
            return Task.CompletedTask;
        }
    }
}
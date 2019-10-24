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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// An interface describing an entity, attached to a <see cref="IGraphField"/> that is capable of
    /// fulfilling a data request for the field. The implementation and expected output will vary from field to field.
    /// </summary>
    public interface IGraphFieldResolver
    {
        /// <summary>
        /// Executes the given context in an attempt to resolve the request and produce a reslt.
        /// </summary>
        /// <param name="context">The field context containing the necessary data to resolve
        /// the field and produce a reslt.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        Task Resolve(FieldResolutionContext context, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the concrete type this resolver attempts to create as a during its invocation (the data type it returns).
        /// If this resolver may generate a list, this type should represent a single list item. (i.e. 'string' not 'List{string}').
        /// </summary>
        /// <value>The type of the return.</value>
        Type ObjectType { get; }
    }
}
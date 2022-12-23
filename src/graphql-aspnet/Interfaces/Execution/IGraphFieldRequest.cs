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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A request passed to a field resolver to complete a custom operation in an effort to generate
    /// a piece of data requested by a user.
    /// </summary>
    public interface IGraphFieldRequest : IDataRequest
    {
        /// <summary>
        /// Gets data item targeted by this request. The contents of the data container
        /// will be different depending on the invocation context.
        /// </summary>
        /// <value>The data item that is the target of this invocation request.</value>
        GraphDataContainer Data { get; }

        /// <summary>
        /// Gets the original operation request governing this field request.
        /// </summary>
        /// <value>The operation request.</value>
        public IQueryOperationRequest OperationRequest { get; }

        /// <summary>
        /// Gets the invocation context, an object containing details about
        /// the invocation of the <see cref="Field"/>.
        /// </summary>
        /// <value>The invocation context governing the field resolution.</value>
        IGraphFieldInvocationContext InvocationContext { get; }

        /// <summary>
        /// Gets the field targeted by the invocation context of the request.
        /// </summary>
        /// <value>The field being resolved.</value>
        IGraphField Field { get; }

        /// <summary>
        /// Gets a set of user-driven key/value pairs shared between all contexts during a
        /// single query execution.
        /// </summary>
        /// <remarks>
        /// This is a collection for developer use and is not used by graphql.
        /// </remarks>
        /// <value>The items collection by this query execution.</value>
        MetaDataCollection Items { get; }
    }
}
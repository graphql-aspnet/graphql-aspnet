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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A request passed to a field resolver to complete a custom operation in an effort to generate
    /// a piece of data requested by a user.
    /// </summary>
    public interface IGraphFieldRequest : IInvocationRequest
    {
        /// <summary>
        /// Gets the parent operation request governing this field request.
        /// </summary>
        /// <value>The operation request.</value>
        public IGraphOperationRequest OperationRequest { get; }

        /// <summary>
        /// Gets the invocation context.
        /// </summary>
        /// <value>The invocation context.</value>
        IGraphFieldInvocationContext InvocationContext { get; }

        /// <summary>
        /// Gets the field targeted by the invocation context of the request.
        /// </summary>
        /// <value>The field.</value>
        IGraphField Field { get; }
    }
}
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A request to a invoke some action to perform some meaningful work against a
    /// data item (or set of data items) that produces an expected result.
    /// </summary>
    public interface IInvocationRequest : IDataRequest
    {
        /// <summary>
        /// Gets data item targeted by this request. The contents of the data container
        /// will be different depending on the invocation context.
        /// </summary>
        /// <value>The data item that is the target of this invocation request.</value>
        GraphDataContainer Data { get; }
    }
}
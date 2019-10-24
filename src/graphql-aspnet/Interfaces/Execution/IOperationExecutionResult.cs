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
    using GraphQL.AspNet.Interfaces.Response;

    /// <summary>
    /// A result to completing the execution of an operation in a graphql query.
    /// </summary>
    public interface IOperationExecutionResult
    {
        /// <summary>
        /// Gets the top level field set generated as a result of executing the operating.
        /// </summary>
        /// <value>The data item generated.</value>
        IResponseFieldSet Data { get; }

        /// <summary>
        /// Gets a collection of messages that may have occured during the resolution of the data request.
        /// </summary>
        /// <value>The collection of messages.</value>
        IGraphMessageCollection Messages { get; }
    }
}
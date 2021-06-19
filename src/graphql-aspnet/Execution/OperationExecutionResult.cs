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
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Response;

    /// <summary>
    /// An instance of a result to executing an graphql operation.
    /// </summary>
    [DebuggerDisplay("Messages = {Messages.Count}")]
    public class OperationExecutionResult : IOperationExecutionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationExecutionResult"/> class.
        /// </summary>
        public OperationExecutionResult()
        {
            this.Messages = new GraphMessageCollection();
        }

        /// <summary>
        /// Sets the completed data response to the result.
        /// </summary>
        /// <param name="data">The data.</param>
        public void SetDataResponse(IResponseFieldSet data)
        {
            this.Data = data;
        }

         /// <summary>
        /// Gets the top level field set generated as a result of executing the operating.
        /// </summary>
        /// <value>The data item generated.</value>
        public IResponseFieldSet Data { get; private set; }

        /// <summary>
        /// Gets a collection of messages that may have occured during the resolution of the data request.
        /// </summary>
        /// <value>The collection of messages.</value>
        public IGraphMessageCollection Messages { get; }
    }
}
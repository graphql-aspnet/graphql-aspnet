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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A request to execute a query through the runtime.
    /// </summary>
    public interface IQueryOperationRequest : IMetaDataContainer
    {
        /// <summary>
        /// Extracts a raw data package from this request.
        /// </summary>
        /// <returns>GraphQueryData.</returns>
        GraphQueryData ToDataPackage();

        /// <summary>
        /// Gets a globally unique identifier assigned to this request when it was created.
        /// </summary>
        /// <value>The identifier of this request.</value>
        Guid Id { get; }

        /// <summary>
        /// Gets the name of the operation, from the supplied query document, to execute.
        /// </summary>
        /// <value>The name of the operation.</value>
        string OperationName { get; }

        /// <summary>
        /// Gets the query text that was supplied by the end user to be parsed and processed.
        /// </summary>
        /// <value>The query text.</value>
        string QueryText { get; }

        /// <summary>
        /// Gets the variables, if any, supplied by the end user.
        /// </summary>
        /// <value>The variables.</value>
        IInputVariableCollection VariableData { get; }

        /// <summary>
        /// Gets the start time, in UTC-0, when this operation began.
        /// </summary>
        /// <value>The start time UTC.</value>
        DateTimeOffset StartTimeUTC { get; }
    }
}
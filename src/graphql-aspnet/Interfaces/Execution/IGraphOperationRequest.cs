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
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A context object representing a single request, by a single user through the query execution.
    /// </summary>
    public interface IGraphOperationRequest : IGraphQLRequest
    {
        /// <summary>
        /// Extracts a raw data package from this request.
        /// </summary>
        /// <returns>GraphQueryData.</returns>
        GraphQueryData ToDataPackage();

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
    }
}
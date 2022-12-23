// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.Response
{
    /// <summary>
    /// An interface representing a single scalar or enum value
    /// to be included in a GraphQL response.
    /// </summary>
    internal interface IQueryResponseSingleValue : IQueryResponseItem
    {
        /// <summary>
        /// Gets the generated value for this single item instance.
        /// </summary>
        /// <value>A value to be included in a query response.</value>
        object Value { get; }
    }
}
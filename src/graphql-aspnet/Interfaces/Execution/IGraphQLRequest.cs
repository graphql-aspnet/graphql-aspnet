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

    /// <summary>
    /// A uniquely identified request for data.
    /// </summary>
    public interface IGraphQLRequest
    {
        /// <summary>
        /// Gets a globally unique identifier assigned to this request when it was created.
        /// </summary>
        /// <value>The identifier.</value>
        Guid Id { get; }
    }
}
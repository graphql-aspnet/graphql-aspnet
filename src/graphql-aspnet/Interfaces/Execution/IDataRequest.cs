﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using GraphQL.AspNet.Execution.Source;

    /// <summary>
    /// A base request defining fields common to all intermediate request types.
    /// </summary>
    public interface IDataRequest : IGraphQLRequest
    {
        /// <summary>
        /// Gets the origin point in the source text where this request was generated.
        /// </summary>
        /// <value>The origin.</value>
        SourceOrigin Origin { get; }
    }
}
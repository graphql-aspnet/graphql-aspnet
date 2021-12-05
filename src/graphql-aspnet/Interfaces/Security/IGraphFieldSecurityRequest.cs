﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Security
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An request to authentication and authorize a user to a field of data.
    /// </summary>
    public interface IGraphFieldSecurityRequest : IDataRequest
    {
        /// <summary>
        /// Gets the field being queried with this request.
        /// </summary>
        /// <value>The field.</value>
        IGraphField Field { get; }
    }
}
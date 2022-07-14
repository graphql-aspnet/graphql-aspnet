// *************************************************************
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
    /// An request to authentication and authorize a user to a schema item.
    /// </summary>
    public interface IGraphSchemaItemSecurityRequest : IDataRequest
    {
        /// <summary>
        /// Gets the secured schema item being queried with this request.
        /// </summary>
        /// <value>The field.</value>
        ISecureSchemaItem SecureSchemaItem { get; }
    }
}
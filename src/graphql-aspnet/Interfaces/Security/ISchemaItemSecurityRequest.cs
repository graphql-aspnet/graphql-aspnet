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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An request to authentication and authorize a user to a schema item.
    /// </summary>
    public interface ISchemaItemSecurityRequest : IDataRequest
    {
        /// <summary>
        /// Gets the secured schema item being checked with this request.
        /// </summary>
        /// <value>The field.</value>
        ISecurableSchemaItem SecureSchemaItem { get; }
    }
}
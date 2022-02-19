// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using GraphQL.AspNet.Security;

    /// <summary>
    /// An interace describing an item as being secure and requiring authorization to access the graph types declared through it.
    /// </summary>
    public interface ISecureItem
    {
        /// <summary>
        /// Gets the security policies found via defined attributes on the item that need to be enforced.
        /// </summary>
        /// <value>The security policies.</value>
        AppliedSecurityPolicyGroup SecurityPolicies { get; }
    }
}
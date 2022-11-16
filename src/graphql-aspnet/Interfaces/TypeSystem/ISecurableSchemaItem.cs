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
    using System.Collections.Generic;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A schema item that implements some security parameters.
    /// </summary>
    public interface ISecurableSchemaItem : ISchemaItem
    {
        /// <summary>
        /// Gets the security groups, a collection of policy requirements, of which each must be met,
        /// in order to access this schema item.
        /// </summary>
        /// <value>The security groups.</value>
        IAppliedSecurityPolicyGroups SecurityGroups { get; }
    }
}
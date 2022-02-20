// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Security
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// A formal definition of policy enforced against an entity.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class EnforcedSecurityPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnforcedSecurityPolicy" /> class.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <param name="policy">The policy definition being enforced.</param>
        public EnforcedSecurityPolicy(string name, AuthorizationPolicy policy)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.Policy = Validation.ThrowIfNullOrReturn(policy, nameof(policy));
        }

        /// <summary>
        /// Gets the name of the enforced policy.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the policy definition.
        /// </summary>
        /// <value>The policy.</value>
        public AuthorizationPolicy Policy { get; }
    }
}
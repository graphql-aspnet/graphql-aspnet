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
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// The application of a policy or set of roles derived from an attribute implementing <see cref="IAuthorizeData"/>
    /// against an entity such as a controller or action method.
    /// </summary>
    public class AppliedSecurityPolicy
    {
        private readonly IAuthorizeData _authData;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedSecurityPolicy"/> class.
        /// </summary>
        /// <param name="authorizationData">The authorization data.</param>
        public AppliedSecurityPolicy(IAuthorizeData authorizationData)
        {
            _authData = Validation.ThrowIfNullOrReturn(authorizationData, nameof(authorizationData));

            this.AllowedRoles = this.ParseCSV(_authData.Roles);
            this.AuthenticationSchemes = this.ParseCSV(_authData.AuthenticationSchemes);
            this.IsNamedPolicy = !string.IsNullOrWhiteSpace(this.PolicyName);
        }

        private ImmutableHashSet<string> ParseCSV(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return ImmutableHashSet.Create<string>();

            return ImmutableHashSet.Create(
                    data.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x?.Trim())
                        .ToArray());
        }

        /// <summary>
        /// Gets the name of the policy.
        /// </summary>
        /// <value>The name of the policy.</value>
        public string PolicyName => _authData.Policy;

        /// <summary>
        /// Gets a value indicating whether this security policy is based on a named authorization policy.
        /// </summary>
        /// <value><c>true</c> if this instance is policy based; otherwise, <c>false</c>.</value>
        public bool IsNamedPolicy { get; }

        /// <summary>
        /// Gets a collection of roles of which a user must be a member of at least 1 to successfully authenticate.
        /// </summary>
        /// <value>The allowed roles.</value>
        public ImmutableHashSet<string> AllowedRoles { get; }

        /// <summary>
        /// Gets a collection of authentication schemes of which the user must have been authenticated by at least 1 to successfully
        /// authorize to this field policy.
        /// </summary>
        /// <value>The authentication schemes.</value>
        public ImmutableHashSet<string> AuthenticationSchemes { get; }
    }
}
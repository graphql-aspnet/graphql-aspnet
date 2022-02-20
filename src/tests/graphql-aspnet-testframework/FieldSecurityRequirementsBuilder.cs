// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Security;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// A builder that can generate a <see cref="FieldSecurityRequirements"/>
    /// object.
    /// </summary>
    public class FieldSecurityRequirementsBuilder
    {
        private HashSet<string> _schemes;
        private List<IEnumerable<string>> _roles;
        private List<EnforcedSecurityPolicy> _policies;
        private bool _allowAnonymous = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSecurityRequirementsBuilder"/> class.
        /// </summary>
        public FieldSecurityRequirementsBuilder()
        {
            _schemes = new HashSet<string>();
            _roles = new List<IEnumerable<string>>();
            _policies = new List<EnforcedSecurityPolicy>();
        }

        /// <summary>
        /// Adds the authentication scheme as one that is allowed
        /// for the user.
        /// </summary>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="policy">The policy.</param>
        /// <returns>FieldSecurityRequirementsBuilder.</returns>
        public FieldSecurityRequirementsBuilder AddEnforcePolicy(string policyName, AuthorizationPolicy policy)
        {
            _policies.Add(new EnforcedSecurityPolicy(policyName, policy));
            return this;
        }

        /// <summary>
        /// Adds the authentication scheme as one that is allowed
        /// for the user.
        /// </summary>
        /// <param name="scheme">The scheme to add.</param>
        /// <returns>FieldSecurityRequirementsBuilder.</returns>
        public FieldSecurityRequirementsBuilder AddAllowedAuthenticationScheme(string scheme)
        {
            _schemes.Add(scheme);
            return this;
        }

        /// <summary>
        /// Adds a set of roles as a group, of which the user must be a member
        /// of at least one.
        /// </summary>
        /// <param name="roles">The roles to add.</param>
        /// <returns>FieldSecurityRequirementsBuilder.</returns>
        public FieldSecurityRequirementsBuilder AddRequiredRoleGroup(params string[] roles)
        {
            _roles.Add(roles);
            return this;
        }

        /// <summary>
        /// allows the requirements item to consider anonymous, unauthenticated users.
        /// </summary>
        /// <returns>FieldSecurityRequirementsBuilder.</returns>
        public FieldSecurityRequirementsBuilder AllowAnonymousUsers()
        {
            _allowAnonymous = true;
            return this;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns>GraphQL.AspNet.Security.FieldSecurityRequirements.</returns>
        public FieldSecurityRequirements Build()
        {
            var allowedSchemes = new List<AllowedAuthenticationScheme>();

            foreach (var scheme in _schemes)
                allowedSchemes.Add(new AllowedAuthenticationScheme(scheme));

            return FieldSecurityRequirements.Create(
                _allowAnonymous,
                allowedSchemes,
                _policies,
                _roles);
        }
    }
}
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
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// A set of requirements indicating what security parameters are enforced
    /// for a given field of information. This object represents the sum total of the
    /// various levels of security that can ultimately be applied to a field
    /// at runtime.
    /// </summary>
    public class FieldSecurityRequirements
    {
        /// <summary>
        /// Gets a set of requirements that automatically denies and will never approve
        /// any field security request.
        /// </summary>
        /// <value>The automatic deny.</value>
        public static FieldSecurityRequirements AutoDeny { get; } = new FieldSecurityRequirements();

        /// <summary>
        /// Creates a new set of security requirements.
        /// </summary>
        /// <param name="allowAnonymous">if set to <c>true</c> this field
        /// allows anonymous requests against it.</param>
        /// <param name="allowedAuthSchemes">A set of authentication schemes.
        /// The user must be authenticated against one of these scheme to be approved.</param>
        /// <param name="enforcedPolicies">The enforced policies.</param>
        /// <param name="enforcedRoleGroups">The enforced role groups.</param>
        /// <returns>FieldSecurityRequirements.</returns>
        public static FieldSecurityRequirements Create(
            bool allowAnonymous,
            IEnumerable<AllowedAuthenticationScheme> allowedAuthSchemes,
            IEnumerable<EnforcedSecurityPolicy> enforcedPolicies = null,
            IEnumerable<IEnumerable<string>> enforcedRoleGroups = null)
        {
            var result = new FieldSecurityRequirements();

            result.AllowedAuthenticationSchemes = allowedAuthSchemes?.ToList() ?? new List<AllowedAuthenticationScheme>();
            result.EnforcedPolicies = enforcedPolicies?.ToList() ?? new List<EnforcedSecurityPolicy>();
            result.EnforcedRoleGroups = enforcedRoleGroups?.Select(x => x.ToList()).ToList() ?? new List<List<string>>();

            result.AllowAnonymous = allowAnonymous;

            return result;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="FieldSecurityRequirements"/> class from being created.
        /// </summary>
        private FieldSecurityRequirements()
        {
            this.AllowedAuthenticationSchemes = new List<AllowedAuthenticationScheme>();
            this.AllowAnonymous = false;
            this.EnforcedPolicies = new List<EnforcedSecurityPolicy>();
            this.EnforcedRoleGroups = new List<IEnumerable<string>>();
        }

        /// <summary>
        /// Gets an approved set of authentication schemes. The user must
        /// be authenticated from one of these schemes to be approved.
        /// </summary>
        /// <value>The allowed schemes.</value>
        public IReadOnlyList<AllowedAuthenticationScheme> AllowedAuthenticationSchemes { get; private set; }

        /// <summary>
        /// Gets the set of authorization policies enforced for this field.
        /// </summary>
        /// <value>The enforced policies.</value>
        public IReadOnlyList<EnforcedSecurityPolicy> EnforcedPolicies { get; private set; }

        /// <summary>
        /// Gets a set of role groups required for this field. The authenticated user
        /// must belong to at least one role from each group to be authorized.
        /// </summary>
        /// <value>The enforced role groups.</value>
        public IReadOnlyList<IEnumerable<string>> EnforcedRoleGroups { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this field allows anonymous access. When <c>true</c>,
        /// authentication and authorization are skipped for the target field.
        /// </summary>
        /// <value><c>true</c> if anonymous access is allowed to the field; otherwise, <c>false</c>.</value>
        public bool AllowAnonymous { get; private set; }
    }
}
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
        /// <param name="allowDefaultSchemeCheck">if set to <c>true</c> the default authentication scheme
        /// will be checked if other <paramref name="requiredAuthSchemes" /> fail.</param>
        /// <param name="requiredAuthSchemes">A set of authentication schemes
        /// the user must be authenticated against one of these scheme to be approved.
        /// When empty, the default authentication scheme configured for this instance
        /// will be allowed.</param>
        /// <param name="enforcedPolicies">The enforced policies.</param>
        /// <param name="enforcedRoleGroups">The enforced role groups.</param>
        /// <returns>FieldSecurityRequirements.</returns>
        public static FieldSecurityRequirements Create(
        bool allowAnonymous,
        bool allowDefaultSchemeCheck,
        IEnumerable<string> requiredAuthSchemes = null,
        IEnumerable<EnforcedSecurityPolicy> enforcedPolicies = null,
        IEnumerable<IEnumerable<string>> enforcedRoleGroups = null)
        {
            requiredAuthSchemes = requiredAuthSchemes?.ToList() ?? Enumerable.Empty<string>();
            enforcedPolicies = enforcedPolicies?.ToList() ?? Enumerable.Empty<EnforcedSecurityPolicy>();
            enforcedRoleGroups = enforcedRoleGroups?.Select(x => x.ToList()).ToList() ?? Enumerable.Empty<IEnumerable<string>>();

            var result = new FieldSecurityRequirements();
            result.AllowAnonymous = allowAnonymous;
            result.AllowDefaultAuthenticationScheme = allowDefaultSchemeCheck;
            result.RequiredAuthenticationSchemes = requiredAuthSchemes;
            result.EnforcedRoleGroups = enforcedRoleGroups;
            result.EnforcedPolicies = enforcedPolicies;

            return result;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="FieldSecurityRequirements"/> class from being created.
        /// </summary>
        private FieldSecurityRequirements()
        {
            this.RequiredAuthenticationSchemes = new List<string>();
            this.AllowDefaultAuthenticationScheme = false;
            this.AllowAnonymous = false;
            this.EnforcedPolicies = new List<EnforcedSecurityPolicy>();
            this.EnforcedRoleGroups = new List<IEnumerable<string>>();
        }

        /// <summary>
        /// Gets an approved set of authentication schemes. The user must
        /// be authenticated from one of these schemes.
        /// </summary>
        /// <value>The allowed schemes.</value>
        public IEnumerable<string> RequiredAuthenticationSchemes { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the default authentication scheme, if one exists,
        /// can be used to authenticate to the field. When false, if the user does not
        /// match one of the <see cref="RequiredAuthenticationSchemes"/> authentication fails.
        /// </summary>
        /// <value><c>true</c> if the default authentication scheme can be used; otherwise, <c>false</c>.</value>
        public bool AllowDefaultAuthenticationScheme { get; private set; }

        /// <summary>
        /// Gets the set of authorization policies enforced for this field.
        /// </summary>
        /// <value>The enforced policies.</value>
        public IEnumerable<EnforcedSecurityPolicy> EnforcedPolicies { get; private set; }

        /// <summary>
        /// Gets a set of role groups required for this field. The authenticated user
        /// must belong to at least one role from each group to be authorized.
        /// </summary>
        /// <value>The enforced role groups.</value>
        public IEnumerable<IEnumerable<string>> EnforcedRoleGroups { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this field allows anonymous access. When <c>true</c>,
        /// authentication and authorization are skipped for the target field.
        /// </summary>
        /// <value><c>true</c> if anonymous access is allowed to the field; otherwise, <c>false</c>.</value>
        public bool AllowAnonymous { get; private set; }
    }
}
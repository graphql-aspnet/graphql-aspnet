// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Middleware.FieldSecurity.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Security;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// A piece of middleware, on the authorization pipeline, that will gather the
    /// security groups (and subseqent policies) that should be evaluated for the
    /// target field.
    /// </summary>
    public class FieldSecurityRequirementsMiddleware : IGraphFieldSecurityMiddleware
    {
        private const string DEFAULT_POLICY = "{6134DC1E-7B4C-4C9A-A410-D7322FE42D5C}";

        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly ConcurrentDictionary<IGraphField, FieldSecurityRequirements> _cachedRequirements;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSecurityRequirementsMiddleware"/> class.
        /// </summary>
        /// <param name="policyProvider">The policy provider used by this application instance.</param>
        public FieldSecurityRequirementsMiddleware(IAuthorizationPolicyProvider policyProvider)
        {
            _policyProvider = policyProvider;
            _cachedRequirements = new ConcurrentDictionary<IGraphField, FieldSecurityRequirements>();
        }

        /// <inheritdoc />
        public async Task InvokeAsync(
            GraphFieldSecurityContext context,
            GraphMiddlewareInvocationDelegate<GraphFieldSecurityContext> next,
            CancellationToken cancelToken = default)
        {
            if (context.SecurityRequirements == null)
            {
                (var requirements, var result) = await this.CreateSecurityRequirements(context.Field);
                context.Result = context.Result ?? result;
                context.SecurityRequirements = requirements;
            }

            await next.Invoke(context, cancelToken);
        }

        private async Task<(FieldSecurityRequirements, FieldSecurityChallengeResult)> CreateSecurityRequirements(IGraphField field)
        {
            if (field?.SecurityGroups == null)
                return (FieldSecurityRequirements.AutoDeny, null);

            if (_cachedRequirements.TryGetValue(field, out var requirements))
                return (requirements, null);

            var allowAnonmous = true;

            AuthorizationPolicy defaultPolicy = null;
            if (_policyProvider != null)
                defaultPolicy = await _policyProvider.GetDefaultPolicyAsync();

            var enforcedPolicies = new List<EnforcedSecurityPolicy>();
            var schemeGroups = new List<IEnumerable<AllowedAuthenticationScheme>>();
            var enforcedRoleGroups = new List<IEnumerable<string>>();

            foreach (var group in field.SecurityGroups)
            {
                // determine if anonmous is allowed trhough the whole
                // field stack...continue to flesh out the required policies
                // regardless
                allowAnonmous = group.AllowAnonymous && group.AllowAnonymous;
                foreach (var rule in group)
                {
                    // schemes defined on the rule are applied first
                    if (rule.AuthenticationSchemes.Any())
                        schemeGroups.Add(rule.AuthenticationSchemes.Select(x => new AllowedAuthenticationScheme(x)));

                    // if this rule defines an explicit policy attempt to
                    // find it
                    if (rule.IsNamedPolicy)
                    {
                        if (_policyProvider == null)
                        {
                            var result = FieldSecurityChallengeResult.Fail(
                                $"A named policy '{rule.PolicyName}' has been applied to the field '{field.Route}' but " +
                                $"no policy provider is configured that can handle it.");

                            return (null, result);
                        }

                        var policy = await _policyProvider.GetPolicyAsync(rule.PolicyName);
                        if (policy == null)
                        {
                            var result = FieldSecurityChallengeResult.Fail(
                                $"The field '{field.Route}' named policy '{rule.PolicyName}' has been applied to the field '{field.Name}' but " +
                                $"no policy provider is configured that can handle it.");

                            return (null, result);
                        }

                        // enforce any scheme requirements on the policy
                        if (policy.AuthenticationSchemes.Count > 0)
                            schemeGroups.Add(policy.AuthenticationSchemes.Select(x => new AllowedAuthenticationScheme(x)));

                        enforcedPolicies.Add(new EnforcedSecurityPolicy(rule.PolicyName, policy));
                    }
                    else if (defaultPolicy != null)
                    {
                        // no policy named, add in the default policy schemes for the app domain
                        if (defaultPolicy.AuthenticationSchemes.Count > 0)
                            schemeGroups.Add(defaultPolicy.AuthenticationSchemes.Select(x => new AllowedAuthenticationScheme(x)));

                        enforcedPolicies.Add(new EnforcedSecurityPolicy(DEFAULT_POLICY, defaultPolicy));
                    }

                    if (rule.AllowedRoles.Count > 0)
                        enforcedRoleGroups.Add(rule.AllowedRoles);
                }
            }

            // only when no required schemes are ever supplied
            // can we allow the fallback default
            var allowDefaultSchemeFallThrough = schemeGroups.Count == 0;

            requirements = FieldSecurityRequirements.Create(
                allowAnonmous,
                this.DeteremineFinalSchemeSet(schemeGroups),
                this.DeteremineFinalPolicySet(enforcedPolicies),
                enforcedRoleGroups);

            // ensure that there exists a scenario where its possible that
            // someone could be authenticated
            if (!requirements.AllowAnonymous
                && !requirements.AllowedAuthenticationSchemes.Any()
                && !allowDefaultSchemeFallThrough)
            {
                var result = FieldSecurityChallengeResult.Fail(
                    $"The field '{field.Route}' has mismatched required authentication schemes in its applied security groups. It contains " +
                    $"no scenarios where an authentication scheme can be used to authenticate a user to all possible required authorizations.");

                return (null, result);
            }

            _cachedRequirements.TryAdd(field, requirements);
            return (requirements, null);
        }

        private IEnumerable<EnforcedSecurityPolicy> DeteremineFinalPolicySet(List<EnforcedSecurityPolicy> enforcedPolicies)
        {
            // if a user passes a policy once they always pass it
            // no need to force them to execute it twice
            // this can happen if multiple [Authorize] attributes
            // are encountered with no named policy
            var enforcedAlready = new HashSet<string>();
            foreach (var policy in enforcedPolicies)
            {
                if (enforcedAlready.Contains(policy.Name))
                    continue;

                enforcedAlready.Add(policy.Name);
                yield return policy;
            }
        }

        private IEnumerable<AllowedAuthenticationScheme> DeteremineFinalSchemeSet(List<IEnumerable<AllowedAuthenticationScheme>> schemeGroups)
        {
            if (schemeGroups.Count == 0)
                return Enumerable.Empty<AllowedAuthenticationScheme>();

            // establish a list of schemes that are required
            // via the first level of security
            var allowedSchemes = new HashSet<AllowedAuthenticationScheme>(AllowedAuthenticationScheme.DefaultComparer);
            foreach (var scheme in schemeGroups.First())
            {
                allowedSchemes.Add(scheme);
            }

            foreach (var schemeGroup in schemeGroups.Skip(1))
            {
                // when required schemes are encounted at lower levels
                // ensure that at least one of the encounted schemes
                // is already in list, if its not then the path to authenticate
                // this field (with a single auth handler) can never be achieved.
                var matchedSchemes = new HashSet<AllowedAuthenticationScheme>(AllowedAuthenticationScheme.DefaultComparer);
                foreach (var scheme in schemeGroup)
                {
                    if (allowedSchemes.Contains(scheme))
                        matchedSchemes.Add(scheme);
                }

                if (matchedSchemes.Count == 0)
                {
                    allowedSchemes.Clear();
                    break;
                }
                else
                {
                    // reduce the "allowed schemes" to just those that those at this
                    // security level
                    allowedSchemes = matchedSchemes;
                }
            }

            return allowedSchemes;
        }
    }
}
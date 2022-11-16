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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// The default implementation of the logic behind a collected set of <see cref="AppliedSecurityPolicyGroup"/>
    /// items.
    /// </summary>
    public class AppliedSecurityPolicyGroups : IAppliedSecurityPolicyGroups
    {
        private readonly List<AppliedSecurityPolicyGroup> _allGroups;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedSecurityPolicyGroups"/> class.
        /// </summary>
        /// <param name="policyGroups">The policy groups to add to this instance.</param>
        public AppliedSecurityPolicyGroups(IEnumerable<AppliedSecurityPolicyGroup> policyGroups = null)
        {
            _allGroups = new List<AppliedSecurityPolicyGroup>(policyGroups ?? Enumerable.Empty<AppliedSecurityPolicyGroup>());

            this.HasSecurityChecks = _allGroups.Sum(x => x.AppliedPolicies.Count) > 0;
        }

        /// <inheritdoc />
        public IEnumerator<AppliedSecurityPolicyGroup> GetEnumerator()
        {
            return _allGroups.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public AppliedSecurityPolicyGroup this[int index] => _allGroups[index];

        /// <inheritdoc />
        public bool HasSecurityChecks { get; }

        /// <inheritdoc />
        public int Count => _allGroups.Count;
    }
}
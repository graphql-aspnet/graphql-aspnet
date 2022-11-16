﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A collection of policy groups applied to a single securable item.
    /// </summary>
    public interface IAppliedSecurityPolicyGroups : IReadOnlyList<AppliedSecurityPolicyGroup>
    {
        /// <summary>
        /// Gets a value indicating whether this collection of policy groups is
        /// such that at least one security check (of some type) is required.
        /// </summary>
        /// <value><c>true</c> if this instance has any security checks; otherwise, <c>false</c>.</value>
        bool HasSecurityChecks { get; }
    }
}
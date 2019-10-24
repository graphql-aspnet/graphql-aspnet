// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers.FieldAuthorizerTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using Microsoft.AspNetCore.Authorization;

    public class Controller_NoPolicies : GraphController
    {
        [Query(typeof(string))]
        [Authorize("RequiresRole1")]
        public Task<IGraphActionResult> GeneralSecureMethod()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }

        [Query(typeof(string))]
        [Authorize("RequiresRole1")]
        public Task<IGraphActionResult> RequireRolePolicy_RequiresRole1()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }

        [Query(typeof(string))]
        [Authorize("RequiresTestClaim6")]
        public Task<IGraphActionResult> RequireClaimPolicy_RequiresTestClaim6()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }

        [AllowAnonymous]
        [Authorize("RequiresTestClaim7")]
        [Query(typeof(string))]
        public Task<IGraphActionResult> RequireClaimPolicy_RequiresTestClaim7_ButAlsoAllowAnon()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }

        [Query(typeof(string))]
        [Authorize]
        public Task<IGraphActionResult> MethodJustAuthorize()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }

        [Query(typeof(string))]
        [Authorize(Roles = "role5")]
        public Task<IGraphActionResult> MethodHasRoles_Role5()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }

        [Query(typeof(string))]
        [Authorize("RequireRole6")]
        public Task<IGraphActionResult> MethodWithRolePolicyRequirement()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }

        [Query(typeof(string))]
        [Authorize("RequireRole6")]
        [Authorize("RequireClaim7")]
        public Task<IGraphActionResult> MultiPolicyMethod_RequireRole6_RequireClaim7()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }

        [Query(typeof(string))]
        public Task<IGraphActionResult> NoDefinedPolicies()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }
    }
}
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.RulesEngine.RuleCheckTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphType("Allow")]
    public class AllowDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FRAGMENT_SPREAD)]
        public IGraphActionResult Execute()
        {
            return null;
        }
    }
}
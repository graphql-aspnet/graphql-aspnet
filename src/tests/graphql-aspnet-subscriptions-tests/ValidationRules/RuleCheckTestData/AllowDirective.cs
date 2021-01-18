// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.Subscriptions.Tests.ValidationRules.RuleCheckTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    [GraphType("Allow")]
    [DirectiveLocations(ExecutableDirectiveLocation.FRAGMENT_SPREAD)]
    public class AllowDirective : GraphDirective
    {
        public IGraphActionResult BeforeFieldResolution()
        {
            return null;
        }
    }
}
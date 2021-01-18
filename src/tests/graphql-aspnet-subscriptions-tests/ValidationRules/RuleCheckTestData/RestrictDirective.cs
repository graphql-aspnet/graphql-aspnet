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

    [GraphType("Restrict")]
    public class RestrictDirective : GraphDirective
    {
        public IGraphActionResult BeforeFieldResolution(int someValue)
        {
            return null;
        }
    }
}
// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Configuration.FormatStrategyTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphType("widgetD")]
    public class WidgetDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD | DirectiveLocation.FRAGMENT_SPREAD | DirectiveLocation.INLINE_FRAGMENT)]
        public IGraphActionResult Execute()
        {
            return this.Ok();
        }
    }
}

// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration.ConfigurationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    [DirectiveLocations(ExecutableDirectiveLocation.AllFieldSelections | ExecutableDirectiveLocation.FRAGMENT_DEFINITION)]
    public class Sample1Directive : GraphDirective
    {
        public IGraphActionResult BeforeFieldResolution()
        {
            return null;
        }
    }
}
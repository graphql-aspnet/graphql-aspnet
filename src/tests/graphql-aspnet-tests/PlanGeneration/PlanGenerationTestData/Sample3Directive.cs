// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.PlanGenerationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    [DirectiveLocations(ExecutableDirectiveLocation.AllFieldSelections)]
    public class Sample3Directive : GraphDirective
    {
        public IGraphActionResult BeforeFieldResolution()
        {
            return null;
        }
    }
}
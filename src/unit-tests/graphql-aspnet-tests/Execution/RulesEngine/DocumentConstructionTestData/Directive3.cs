// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.RulesEngine.DocumentConstructionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class Directive3 : GraphDirective
    {
        [DirectiveLocations(AspNet.Schemas.TypeSystem.DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute(int arg1, string arg2)
        {
            return null;
        }
    }
}
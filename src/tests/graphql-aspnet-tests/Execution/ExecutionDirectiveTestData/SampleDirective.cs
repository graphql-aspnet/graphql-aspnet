// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionDirectiveTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class SampleDirective : GraphDirective
    {
        public List<(DirectiveLocation Location, string Value)> ValuesReceived { get; }

        public SampleDirective()
        {
            this.ValuesReceived = new List<(DirectiveLocation, string)>();
        }

        [DirectiveLocations(DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute([FromGraphQL(TypeExpressions.IsNotNull)] string arg1)
        {
            this.ValuesReceived.Add((this.DirectiveLocation, arg1));
            return this.Ok();
        }
    }
}
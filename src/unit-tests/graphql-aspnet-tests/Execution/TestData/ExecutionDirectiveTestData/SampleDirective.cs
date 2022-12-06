// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionDirectiveTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class SampleDirective : GraphDirective
    {
        public List<(DirectiveLocation Location, DirectiveInvocationPhase Phase, string Value)> ValuesReceived { get; }

        public SampleDirective()
        {
            this.ValuesReceived = new List<(DirectiveLocation, DirectiveInvocationPhase, string)>();
        }

        [DirectiveLocations(DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute([FromGraphQL(TypeExpression = "Type!")] string arg1)
        {
            this.ValuesReceived.Add((this.DirectiveLocation, this.DirectivePhase, arg1));
            return arg1 == "abort" ? this.Cancel() : this.Ok();
        }
    }
}
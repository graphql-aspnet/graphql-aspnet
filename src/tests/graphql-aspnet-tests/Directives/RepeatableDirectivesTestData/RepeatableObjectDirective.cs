// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Directives.RepeatableDirectivesTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    [Repeatable]
    public class RepeatableObjectDirective : GraphDirective
    {
        public RepeatableObjectDirective()
        {
            this.SuppliedData = new List<string>();
        }

        [DirectiveLocations(AspNet.Schemas.TypeSystem.DirectiveLocation.OBJECT)]
        public IGraphActionResult Execute(string data)
        {
            this.TotalApplications += 1;
            this.SuppliedData.Add(data);
            return this.Ok();
        }

        public int TotalApplications { get; set; }

        public List<string> SuppliedData { get; set; }
    }
}
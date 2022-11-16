// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospectionTestData
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphRoute("fountain")]
    public class SodaFountainController : GraphController
    {
        [Query(typeof(SodaTypeUnionProxy), TypeExpression = TypeExpressions.IsList)]
        public Task<IGraphActionResult> RetrieveSodaTypes()
        {
            var list = new List<ISodaType>()
            {
                new SodaTypeA() { Name = "TypeA" },
                new SodaTypeB() { Name = "TypeB" },
            };

            return Task.FromResult(this.Ok(list));
        }
    }
}
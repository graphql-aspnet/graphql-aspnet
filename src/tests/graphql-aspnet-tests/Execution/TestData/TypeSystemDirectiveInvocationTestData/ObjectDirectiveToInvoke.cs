// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveInvocationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ObjectDirectiveToInvoke : GraphDirective
    {
        public static int TotalInvocations { get; private set; } = 0;

        [DirectiveLocations(DirectiveLocation.OBJECT)]
        public IGraphActionResult PerformOperationon()
        {
            if (this.DirectiveTarget is IObjectGraphType ogt)
            {
                if (ogt.ObjectType == typeof(ObjectForDirectiveInvocation))
                    TotalInvocations += 1;
            }

            return this.Ok();
        }
    }
}
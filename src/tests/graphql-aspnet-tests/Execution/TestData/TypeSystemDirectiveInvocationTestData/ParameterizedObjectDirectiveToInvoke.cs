// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveInvocationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ParameterizedObjectDirectiveToInvoke : GraphDirective
    {
        public static int TotalInvocations = 0;
        public static string LastParam1Value = string.Empty;
        public static int LastParam2Value = 0;

        [DirectiveLocations(DirectiveLocation.OBJECT)]
        public IGraphActionResult PerformOperationon(string param1, int item2)
        {
            TotalInvocations += 1;
            LastParam1Value = param1;
            LastParam2Value = item2;
            return this.Ok();
        }
    }
}
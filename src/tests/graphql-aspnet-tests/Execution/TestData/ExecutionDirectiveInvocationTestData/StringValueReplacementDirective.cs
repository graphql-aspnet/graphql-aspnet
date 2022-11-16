// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.ExecutionDirectiveInvocationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class StringValueReplacementDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD)]
        public IGraphActionResult Execute()
        {
            if (this.DirectiveTarget?.GetType() == typeof(string))
            {
                this.DirectiveTarget = "REPLACED STRING";
            }

            return this.Ok();
        }
    }
}
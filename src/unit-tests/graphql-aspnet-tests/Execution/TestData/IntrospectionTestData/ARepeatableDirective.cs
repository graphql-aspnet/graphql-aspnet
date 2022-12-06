// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [Repeatable]
    public class ARepeatableDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.SCALAR)]
        public IGraphActionResult ForTypeSystem(int firstArg, TwoPropertyObject secondArg)
        {
            return null;
        }
    }
}
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration.FormatStrategyTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class WidgetArgumentController : GraphController
    {
        [QueryRoot]
        public Widget WithArgument(string arg1)
        {
            return new Widget();
        }

        [QueryRoot(TypeExpression = "Type")]
        public Widget WithArgumentFixed([FromGraphQL(TypeExpression = "Type")] string arg1)
        {
            return new Widget();
        }
    }
}
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
        public Widget IntArgument(int arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget IntArgumentFixed([FromGraphQL(TypeExpression = "Type!")] int arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget StringArgument(string arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget StringArgumentFixed([FromGraphQL(TypeExpression = "Type")] string arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget InputObjectArgument(Widget arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget InputObjectArgumentFixed([FromGraphQL(TypeExpression = "Type")] Widget arg1)
        {
            return new Widget();
        }
    }
}
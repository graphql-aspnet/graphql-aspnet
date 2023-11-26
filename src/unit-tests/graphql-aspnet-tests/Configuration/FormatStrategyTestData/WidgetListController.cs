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
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class WidgetListController : GraphController
    {
        [QueryRoot]
        public Widget IntArgument(List<int> arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget IntArgumentFixed([FromGraphQL(TypeExpression = "[Type!]")]List<int> arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget StringArgument(List<string> arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget StringArgumentFixed([FromGraphQL(TypeExpression = "[Type]")] List<string> arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget InputObjectArgument(List<Widget> arg1)
        {
            return new Widget();
        }

        [QueryRoot]
        public Widget InputObjectArgumentFixed([FromGraphQL(TypeExpression = "[Type]")] List<Widget> arg1)
        {
            return new Widget();
        }
    }
}
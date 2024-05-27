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

    public class WidgetControllerWithDefaultValue : GraphController
    {
        [QueryRoot("retrieveRootWidget")]
        public Widget RetrieveWidgetFromRoot(string arg1 = "default 1")
        {
            return new Widget();
        }
    }
}
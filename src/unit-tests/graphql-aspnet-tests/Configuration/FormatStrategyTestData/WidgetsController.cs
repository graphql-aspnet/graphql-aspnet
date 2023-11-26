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

    public class WidgetsController : GraphController
    {
        [QueryRoot("retrieveRootWidget")]
        public Widget RetrieveWidgetFromRoot()
        {
            return new Widget();
        }

        [Query("/path1/path2/retrieveWidget")]
        public Widget RetrieveWidgetFromTemplatePath()
        {
            return new Widget();
        }
    }
}
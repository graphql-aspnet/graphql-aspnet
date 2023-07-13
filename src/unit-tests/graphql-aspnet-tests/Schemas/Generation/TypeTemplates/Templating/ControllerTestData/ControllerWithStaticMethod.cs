// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ControllerTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class ControllerWithStaticMethod : GraphController
    {
        [Query]
        public static int StaticMethod(int a, int b)
        {
            return a - b;
        }

        [Query]
        public int InstanceMethod(int a, int b)
        {
            return a + b;
        }
    }
}
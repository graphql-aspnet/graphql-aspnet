// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class ArgumentMarkedDirectiveTestController : GraphController
    {
        [Query]
        public int DoThing(
            [ApplyDirective(typeof(ArgumentMarkerDirective))] int inputArg)
        {
            return 0;
        }
    }
}
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTests
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class InputObjectDirectiveTestController : GraphController
    {
        [Query]
        public int DoThing(MarkedInputObject obj)
        {
            return 0;
        }
    }
}
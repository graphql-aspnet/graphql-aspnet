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
    using GraphQL.AspNet.Tests.CommonHelpers;

    /// <summary>
    /// contains an overloaded method without providing specific names for each in the
    /// object graph.
    /// </summary>
    public class TwoMethodClashController : GraphController
    {
        [Query]
        public TwoPropertyObject ActionMethodNoAttributes()
        {
            return new TwoPropertyObject();
        }

        [Query]
        public TwoPropertyObject ActionMethodNoAttributes(string input1)
        {
            return new TwoPropertyObject();
        }
    }
}